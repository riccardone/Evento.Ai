﻿using Azure.Messaging.ServiceBus;
using Evento.Ai.Chatter;
using Evento.Ai.Processor;
using Evento.Ai.Processor.Adapter;
using Evento.Ai.Processor.Domain.Services;
using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using OpenAI;
using System.Text.Json;

namespace Evento.Ai.Host;

internal class Program
{
    static void Main(string[] args)
    {
        var logger = LogManager.GetCurrentClassLogger();
        try
        {
            if (args.Length == 0)
                CreateHostBuilder(args).Build().Run();
            else
            {
                logger.Info("Discovering validation schema...");
                IConfiguration configuration = BuildConfig();
                var settings = configuration.Get<Settings>();
                var openAiClient = new OpenAIClient(new OpenAIAuthentication(settings.OpenAIApiKey, settings.OpenAIOrganization));
                var chatter = new OpenAiChatter(openAiClient);
                var chatterService = new ChatterService(chatter);
                logger.Info($"Talking with AI about: {args[0]}");
                var validationSchema = chatterService.GetValidationSchema(args[0]);
                logger.Info("-------------------------");
                logger.Info($"Schema Name: {validationSchema.Id}");
                logger.Info("-------------------------");
                logger.Info(PrettyPrintJsonDocument(validationSchema.Data));
                logger.Info("-------------------------");
            }
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

    private static string PrettyPrintJsonDocument(JsonDocument doc)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true 
        };
        return JsonSerializer.Serialize(doc.RootElement, options);
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
                services.AddSingleton(configuration);
                var sbClient = new ServiceBusClient(settings.ServiceBusConnectionString);
                var processor = sbClient.CreateSessionProcessor(settings.QueueName, new ServiceBusSessionProcessorOptions()
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentSessions = 32,
                    PrefetchCount = 32
                });
                services.AddSingleton<IConnectionBuilder>(
                    _ => BuilderForDomain($"{settings.CloudRequestSource}-conn", settings,
                        ConnectionBuilder.BuildConnectionSettings(settings), logger));
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

    private static IConnectionBuilder BuilderForDomain(string connectionName, Settings settings,
        ConnectionSettings connSettings, NLog.ILogger logger)
    {
        var builderForDomain = new ConnectionBuilder(new Uri(settings.EventStoreConnectionString), connSettings,
            connectionName, logger);
        return builderForDomain;
    }
}