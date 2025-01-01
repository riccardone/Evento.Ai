using Azure.Messaging.ServiceBus;
using Evento.Ai.Brain;
using Evento.Ai.Contracts;
using Evento.Ai.Listner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Evento.Ai.Host;

internal class Program
{
    static void Main(string[] args)
    {
        var logger = LogManager.GetCurrentClassLogger();
        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception e)
        {
            logger.Error(e, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var hostName = "*";
        if (args.Length > 0) hostName = args[0];
        IConfiguration configuration = BuildConfig();

        return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddNLog(configuration); logging.AddConsole();
                logging.AddFilter("Microsoft.*", Microsoft.Extensions.Logging.LogLevel.Error);
                logging.AddFilter("System.Net.Http.*", Microsoft.Extensions.Logging.LogLevel.Error);
#if DEBUG
                var configPath = "nlog-dev.config";
#else
                var configPath = "nlog.config";
#endif
                LogManager.Setup()
                    .SetupExtensions(e => e.AutoLoadAssemblies(false))
                    .LoadConfigurationFromFile(configPath, optional: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var settings = configuration.Get<Settings>();
                var logger = LogManager.GetLogger($"Evento.AI");
                services.AddSingleton<NLog.ILogger>(logger);
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddNLog(new NLogProviderOptions() { ShutdownOnDispose = true });
                }); services.AddSingleton(settings);

                services.AddSingleton<Worker>();
                services.AddSingleton<IHostedService>(p => p.GetRequiredService<Worker>());
                services.AddSingleton(configuration);
                services.AddSingleton<IBrainUnit<BrainUnit>, BrainUnit>();
                var sbClient = new ServiceBusClient(settings.ServiceBusConnectionString);
                var processor = sbClient.CreateSessionProcessor(settings.QueueName, new ServiceBusSessionProcessorOptions()
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentSessions = 32,
                    PrefetchCount = 32
                });
                services.AddSingleton(processor);
                services.AddHostedService(e => e.GetRequiredService<Application>());
                //services.AddSingleton<IHttpService, HttpService>();
#if !DEBUG
                services.AddCustomTinyHealthCheck<CustomHealthCheck>(config =>
                {
                    config.Port = 3000;
                    config.Hostname = hostName;
                    return config;
                });
#endif
            });
    }

    private static IConfigurationRoot BuildConfig()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev";
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
        return builder.Build();
    }
}