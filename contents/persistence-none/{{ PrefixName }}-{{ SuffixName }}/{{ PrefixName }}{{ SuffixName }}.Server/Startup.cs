
using CorrelationId;
using CorrelationId.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using {{ PrefixName }}{{ SuffixName }}.Core.Services;
using {{ PrefixName }}{{ SuffixName }}.Server.Authorization;
using {{ PrefixName }}{{ SuffixName }}.Server.GraphQL;
using {{ PrefixName }}{{ SuffixName }}.Server.HealthChecks;
using {{ PrefixName }}{{ SuffixName }}.Server.Persistence.Context;
using {{ PrefixName }}{{ SuffixName }}.Server.Services;
using System.Text;

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
        // Add controllers
        services.AddControllers();

        // Add CORS
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        // Add database context
        var useTempDb = _configuration.GetValue<bool>("TEMP_DB");
        if (useTempDb)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TempDb"));
        }
        else
        {
            // Fallback to in-memory if no connection string or not configured
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("DefaultDb"));
        }

        // Add authentication
        var disableAuth = _configuration.GetValue<bool>("DISABLE_AUTH");
        if (!disableAuth)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (!string.IsNullOrEmpty(jwtKey))
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = _configuration["Jwt:Issuer"],
                            ValidAudience = _configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                        };
                    });
            }
        }

        services.AddAuthorization();

        // Add services
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IAuthenticationService, JwtAuthenticationService>();
        services.AddScoped<IApiGatewayJwtValidator, ApiGatewayJwtValidator>();
        services.AddSingleton<MetricsService>();
        services.AddHostedService<GracefulShutdownService>();

        // Add ephemeral database service
        services.AddSingleton<EphemeralDatabaseService>();

        // GraphQL authorization handler is registered in GraphQL configuration

        // Add health checks
        services.AddHealthChecks()
            .AddCheck<ServiceHealthCheck>("service_health");

        // Add GraphQL
        var graphqlBuilder = services.AddGraphQLServer()
            .AddQueryType<{{ PrefixName }}Queries>()
            .AddMutationType<{{ PrefixName }}Mutations>()
            .AddErrorFilter<GraphQLErrorFilter>()
            .AddDiagnosticEventListener<GraphQLDiagnosticEventListener>();

        // Add authorization with test handler if auth is disabled
        if (disableAuth)
        {
            graphqlBuilder.Services.AddSingleton<HotChocolate.Authorization.IAuthorizationHandler, TestAuthorizationHandler>();
        }
        graphqlBuilder.AddAuthorization();

        // Add Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "{{ PrefixName }}{{ SuffixName }} API",
                Version = "v1",
                Description = "{{ PrefixName }} Service API"
            });

            // Add JWT authentication to Swagger
            if (!disableAuth)
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            }
        });

        // Add OpenTelemetry
        var serviceName = _configuration["ServiceName"] ?? "{{ PrefixName }}{{ SuffixName }}";
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();
            });

        // Add correlation ID
        services.AddDefaultCorrelationId();
    }

    public void Configure(IApplicationBuilder app)
    {
        // Use correlation ID
        app.UseCorrelationId();

        // Use routing
        app.UseRouting();

        // Use CORS
        app.UseCors();

        // Use authentication/authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Use Swagger
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "{{ PrefixName }}{{ SuffixName }} API v1");
            c.RoutePrefix = "swagger";
        });

        // Use endpoints
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGraphQL("/graphql");
            endpoints.MapHealthChecks("/health");
            endpoints.MapPrometheusScrapingEndpoint("/metrics");
        });
    }
}

