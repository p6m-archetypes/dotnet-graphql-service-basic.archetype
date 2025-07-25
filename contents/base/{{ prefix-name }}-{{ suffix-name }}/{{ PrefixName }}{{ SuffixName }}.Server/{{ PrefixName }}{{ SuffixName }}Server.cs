using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using OpenTelemetry.Logs;
using Serilog;
using {{ PrefixName }}{{ SuffixName }}.Server.Services;

namespace {{ PrefixName }}{{ SuffixName }}.Server;

public class {{ PrefixName }}{{ SuffixName }}Server
{
    private string[] args = [];
    private WebApplication? app;
    private IConfiguration? configuration;

    public {{ PrefixName }}{{ SuffixName }}Server Start()
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure graceful shutdown
        builder.Host.ConfigureHostOptions(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(30);
        });

        builder.Host.UseSerilog((content, loggerConfig) =>
            loggerConfig.ReadFrom.Configuration(builder.Configuration)
        );

        builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter());

        configuration = builder.Configuration;
        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);
        app = builder.Build();

        // Start ephemeral database if in ephemeral mode
        var isEphemeral = builder.Configuration["ASPNETCORE_ENVIRONMENT"] == "Ephemeral" ||
                         builder.Configuration["SPRING_PROFILES_ACTIVE"]?.Contains("ephemeral") == true;

        if (isEphemeral)
        {
            var ephemeralDbService = app.Services.GetRequiredService<EphemeralDatabaseService>();
            ephemeralDbService.StartDatabaseAsync().GetAwaiter().GetResult();
        }

        startup.Configure(app);
        app.Start();

        return this;
    }

    public {{ PrefixName }}{{ SuffixName }}Server Stop()
    {
        if (app != null)
        {
            // Stop ephemeral database if running
            try
            {
                var ephemeralDbService = app.Services.GetService<EphemeralDatabaseService>();
                if (ephemeralDbService != null && ephemeralDbService.IsRunning)
                {
                    ephemeralDbService.StopDatabaseAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to stop ephemeral database: {ex.Message}");
            }

            app.StopAsync().GetAwaiter().GetResult();
        }
        return this;
    }

    public {{ PrefixName }}{{ SuffixName }}Server WithArguments(string[] args)
    {
        this.args = args;
        return this;
    }

    public {{ PrefixName }}{{ SuffixName }}Server WithEphemeral()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Ephemeral");
        Environment.SetEnvironmentVariable("SPRING_PROFILES_ACTIVE", "ephemeral");
        return this;
    }

    public {{ PrefixName }}{{ SuffixName }}Server WithRandomPorts()
    {
        // Use fixed test port for integration tests to avoid port discovery issues
        Environment.SetEnvironmentVariable("HTTP_PORT", "5041");
        return this;
    }

    public string? getGrpcUrl()
    {
        if (app == null) return null;

        try
        {
            var serverAddresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
            // Look for the gRPC endpoint (HTTP/2) - should be the one NOT on port 5031
            var grpcAddress = serverAddresses?.Addresses.FirstOrDefault(addr => !addr.Contains("5031"));

            if (grpcAddress != null)
            {
                // Replace 0.0.0.0 with localhost for client connections
                return grpcAddress.Replace("0.0.0.0", "localhost");
            }

            return "http://localhost:5030";
        }
        catch
        {
            return "http://localhost:5030";
        }
    }

    public IConfiguration GetConfiguration()
    {
        return configuration ?? throw new InvalidOperationException("Server not started yet");
    }

    public WebApplication? GetApp()
    {
        return app;
    }

    public static async Task Main(string[] args)
    {
        var server = new {{ PrefixName }}{{ SuffixName }}Server()
            .WithArguments(args);

        // Parse command-line arguments for special modes
        if (args.Contains("--ephemeral"))
        {
            server.WithEphemeral();
        }


        server.Start();

        // Simulate waiting for shutdown signal or some other condition
        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            // Register an event to stop the app when Ctrl+C or another shutdown signal is received
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true; // Prevent the app from terminating immediately
                cancellationTokenSource.Cancel(); // Trigger the stop signal
            };

            try
            {
                // Wait indefinitely (or for a shutdown signal) by awaiting on the task
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // The delay task is canceled when a shutdown signal is received
            }
        }

        // Gracefully stop the application
        server.Stop();
    }
}