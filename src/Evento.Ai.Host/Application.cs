using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CloudEventData;
using Evento.Ai.Contracts;
using Evento.Ai.Processor.Adapter;
using Evento.Repository;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Evento.Ai.Host;

public class Application : IHostedService
{
    private readonly Settings _settings;
    private readonly ServiceBusSessionProcessor _processor;
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private IEventStoreConnection _connection;
    private IDomainRepository _domainRepository;
    private Worker _worker;
    public bool _started;

    public Application(ServiceBusSessionProcessor processor, Settings settings)
    {
        _processor = processor;
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        _settings = settings;
    }

    public async Task StartAsync(CancellationToken token)
    {
        await _processor.StartProcessingAsync();
        _started = true;
    }

    public async Task StopAsync(CancellationToken token)
    {
        await _processor.StopProcessingAsync();
    }

    private async Task ProcessErrorAsync(ProcessErrorEventArgs e)
    {
        _logger.Error(e.Exception.ToString());
    }

    private async Task ProcessMessageAsync(ProcessSessionMessageEventArgs args)
    {
        try
        {
            _logger.Debug("Triggered...");
            _connection = Build();
            var text = Encoding.UTF8.GetString(args.Message.Body);
            var cloudRequest = JsonSerializer.Deserialize<CloudEventRequest>(text); // TODO test this
            _domainRepository = new EventStoreDomainRepository(_settings.EventCategory, _connection);

            _worker = new Worker(_domainRepository, _logger);
            _worker.Process(cloudRequest);

            await args.CompleteMessageAsync(args.Message);

            _logger.Info($"C# ServiceBus queue trigger function processed message: {args.Message.MessageId}");
        }
        catch (Exception ex)
        {
            var errForServiceBus = ex.GetBaseException().Message.Length > 4096
                ? ex.GetBaseException().Message.Substring(0, 4096)
                : ex.GetBaseException().Message;

            var errDescriptionForServiceBus = ex.GetBaseException().StackTrace.Length > 4096
                ? ex.GetBaseException().StackTrace.Substring(0, 4096)
                : ex.GetBaseException().StackTrace;

            await args.DeadLetterMessageAsync(args.Message, errForServiceBus,
                errDescriptionForServiceBus);
        }
        finally
        {
            _connection.Close();
        }
    }

    private IEventStoreConnection Build(bool openConnection = true)
    {
        //var conn = EventStoreConnection.Create(BuildConnectionSettings(new UserCredentials("...", "...")), "", "");
        //conn.Disconnected += Conn_Disconnected;
        //conn.Reconnecting += Conn_Reconnecting;
        //conn.Connected += Conn_Connected;
        //conn.Closed += Conn_Closed;
        //if (openConnection)
        //    conn.ConnectAsync().Wait();

        //return conn;
        throw new NotImplementedException();
    }

    private static ConnectionSettings BuildConnectionSettings(UserCredentials userCredentials)
    {
        var connectionSettingsBuilder = ConnectionSettings.Create()
            .SetDefaultUserCredentials(userCredentials);
        //.KeepReconnecting().KeepRetrying();
        return connectionSettingsBuilder.Build();
    }
}