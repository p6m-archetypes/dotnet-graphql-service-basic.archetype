
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using {{ PrefixName }}{{ SuffixName }}.Core;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Context;
using {{ PrefixName }}{{ SuffixName }}.Persistence.Repositories;
using {{ PrefixName }}{{ SuffixName }}.Server.GraphQL;
using {{ PrefixName }}{{ SuffixName }}.Server.Services;
using {{ PrefixName }}{{ SuffixName }}.Server.HealthChecks;
using {{ PrefixName }}{{ SuffixName }}.Server.Authorization;
using CorrelationId;
using CorrelationId.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace {{ PrefixName }}{{ SuffixName }}.Server;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Add correlation ID support
        services.AddDefaultCorrelationId();

        // Configure ephemeral database options
        services.Configure<EphemeralDatabaseOptions>(_configuration.GetSection(EphemeralDatabaseOptions.SectionName));

        // Configure database
        var useTempDb = _configuration.GetValue<bool>("TEMP_DB");
        var isEphemeral = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Ephemeral";

        if (useTempDb)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("{{ PrefixName }}{{ SuffixName }}TempDb"));
        }
        else if (isEphemeral)
        {
            // For ephemeral mode, use a factory that gets the connection string from the ephemeral database service
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var ephemeralDb = serviceProvider.GetService<EphemeralDatabaseService>();
                var connString = ephemeralDb?.GetConnectionString()
                    ?? _configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connString);
            });
        }
        else
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        // Register repositories
        services.AddScoped<I{{ PrefixName }}Repository, {{ PrefixName }}Repository>();

        // Register services
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<{{ PrefixName }}{{ SuffixName }}Core>();
        services.AddSingleton<IAuthenticationService, JwtAuthenticationService>();
        services.AddScoped<IApiGatewayJwtValidator, ApiGatewayJwtValidator>();
        services.AddSingleton<MetricsService>();

        // Register ephemeral database service
        services.AddSingleton<EphemeralDatabaseService>();
        services.AddHostedService<EphemeralDatabaseHostedService>();
        services.AddHostedService<GracefulShutdownService>();

        // Configure JWT Authentication
        var secretKey = _configuration["Authentication:Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        // Configure GraphQL
        var graphqlBuilder = services
            .AddGraphQLServer()
            .AddQueryType(d => d.Name("Query"))
            .AddTypeExtension<{{ PrefixName }}Queries>()
            .AddMutationType(d => d.Name("Mutation"))
            .AddTypeExtension<{{ PrefixName }}Mutations>()
            .AddErrorFilter<GraphQLErrorFilter>()
            .AddDiagnosticEventListener<GraphQLDiagnosticEventListener>()
            .ModifyRequestOptions(opt =>
            {
                opt.IncludeExceptionDetails = true;  // Always include exception details to help debugging
            });

        // Configure authorization - always call AddAuthorization() to register the @authorize directive
        var disableAuth = _configuration["DISABLE_AUTH"];
        var env = _configuration["ASPNETCORE_ENVIRONMENT"];

        graphqlBuilder.AddAuthorization();

        // In Ephemeral mode, replace the default authorization handler with TestAuthorizationHandler
        // which will allow all requests
        if (env == "Ephemeral" || disableAuth == "true")
        {
            // Remove the default authorization handler that was just added
            var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(HotChocolate.Authorization.IAuthorizationHandler));
            if (serviceDescriptor != null)
            {
                services.Remove(serviceDescriptor);
            }
            services.AddSingleton<HotChocolate.Authorization.IAuthorizationHandler, TestAuthorizationHandler>();
        }

        // Enable introspection in development and ephemeral environments
        if (env == "Development" || env == "Ephemeral" || disableAuth == "true")
        {
            graphqlBuilder.AllowIntrospection(true);
        }

        // Add health checks
        if (!useTempDb && !isEphemeral)
        {
            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database");
        }
        else
        {
            services.AddHealthChecks();
        }

        // Configure OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: _configuration["Application:Name"] ?? "{{ prefixName }}-service",
                            serviceVersion: _configuration["Application:Version"] ?? "1.0.0"))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddPrometheusExporter())
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation());

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
        });

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        var env = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");

        if (env == "Development" || env == "Ephemeral")
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseCorrelationId();
        app.UseRouting();
        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGraphQL();
            endpoints.MapHealthChecks("/health");
            endpoints.MapPrometheusScrapingEndpoint();
            endpoints.MapControllers();
        });
    }
}

