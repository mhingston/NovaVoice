using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaVoice.Models;
using NovaVoice.Tools;
using Serilog;

namespace NovaVoice;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        
        try
        {
            Log.Information("Starting NovaVoice");
            await CreateHostBuilder(args).Build().RunAsync();
            return 0;
        }
        
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
            return 1;
        }
        
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext())
            .ConfigureAppConfiguration(ConfigureApp)
            .ConfigureServices(ConfigureServices);

    private static void ConfigureApp(HostBuilderContext context, IConfigurationBuilder builder)
    {
        builder.SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                optional: true, 
                reloadOnChange: true)
            .AddEnvironmentVariables("NOVA_")  // Prefix environment variables
            .AddCommandLine(Environment.GetCommandLineArgs());
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Configuration
        services.AddOptions<Configuration>()
            .Bind(context.Configuration)
            .ValidateOnStart();
        services.AddSingleton<IValidator<Configuration>, ConfigurationValidator>();

        // HTTP Clients
        services.AddHttpClients();

        // Tools
        services.AddTools();

        // Core Services
        services.AddSingleton<YoutubePlayer>();
        services.AddHostedService<Assistant>();
    }
}

// Extension methods for better organization
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<GoogleApiClient.GoogleApiClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("NovaVoice/1.0");
            // Add any other common configurations
        });

        services.AddHttpClient<AccuWeatherApiClient.AccuWeatherApiClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("NovaVoice/1.0");
        });

        services.AddHttpClient<GroqApiClient.GroqApiClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("NovaVoice/1.0");
        });

        return services;
    }

    public static IServiceCollection AddTools(this IServiceCollection services)
    {
        services.AddSingleton<ITool, GoogleSearchTool>();
        services.AddSingleton<ITool, YoutubeSearchTool>();
        services.AddSingleton<ITool, WeatherTool>();

        return services;
    }
}