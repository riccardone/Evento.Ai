using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using Evento.Ai.Contracts;
using Evento.Ai.Listner.Neurons;
using NLog;
using Remundo.AI.Contracts;

[assembly: InternalsVisibleTo("Evento.AI.Tests")]
namespace Evento.Ai.Listner;

public class BrainUnit : IBrainUnit<BrainUnit>
{
    //private readonly IDataReader _datareader;
    //private readonly ICommandSender _commandSender;
    private readonly ILogger _logger;
    //private IEventStoreConnection? _conn;
    private readonly Dictionary<string, Action<JsonNode, JsonNode>> _handlingFunctions;
    //private readonly CatchUpSubscriptionSettings _catchUpSubscriptionSettings = new(2000, 20, false, true);
    //private Position _catchUpFrom;
    //private IPositionRepository? _positionRepository;
    private readonly Settings _settings;
    //private IChatter _chatter;
    private readonly IDictionary<string, Neuron> _neurons;

// TODO contructors

    private IDictionary<string, Neuron> CreateNeurons()
    {
        return new Dictionary<string, Neuron>
        {
            //{ nameof(Something), new Something() }
        };
    }

    private Dictionary<string, Action<JsonNode, JsonNode>> CreateHandlers()
    {
        return new Dictionary<string, Action<JsonNode, JsonNode>>
        {
            //{ "eventnamehere", HandleEventHere }
        };
    }

    
    #region BoilerplateCode

    public async Task StartAsync()
    {
        //_conn = BuildConnection(
        //    new Uri(_settings.EventStoreConnectionString), BuildConnectionSettings(), nameof(BrainUnit));
        //_conn.Connected += _conn_Connected;
        //if (!_settings.Volatile)
        //{
        //    _positionRepository = new PositionRepository(
        //        $"{nameof(BrainUnit)}Position", "PositionSaved",
        //        () => BuildConnection(new Uri(_settings.EventStoreConnectionString),
        //            BuildConnectionSettings(),
        //            $"{nameof(BrainUnit)}-position-{Guid.NewGuid().GetHashCode()}"),
        //        _settings.IntervalToSaveLastPosition);
        //    _catchUpFrom = _settings.CatchUpFrom == 0 ? Position.Start : Position.End;
        //    await _positionRepository.Start();
        //}
        //var openAiClient = new OpenAIClient(new OpenAIAuthentication(_settings.OpenAIApiKey, _settings.OpenAIOrganization));
        //_chatter = new OpenAiChatter(openAiClient);
        //await _conn.ConnectAsync();
        //_logger.Info(
        //    _settings.Volatile ? $"Started volatile subscription of {nameof(Negotiator)}"
        //                       : $"Started catch-up subscription of {nameof(Negotiator)}");
    }

    public Task StopAsync()
    {
        //_conn?.Close();
        //_positionRepository?.Stop();
        return Task.CompletedTask;
    }

    //private void _conn_Connected(object? sender, ClientConnectionEventArgs e)
    //{
    //    _logger?.Info($"{GetType().FullName} Connected to EventStore");
    //    SubscribeMe();
    //}

    //private void SubscribeMe()
    //{
    //    if (_settings.Volatile)
    //        _conn?.SubscribeToAllAsync(true, EventAppeared, SubscriptionDropped);
    //    else
    //    {
    //        _conn?.SubscribeToAllFrom(_catchUpFrom, _catchUpSubscriptionSettings, EventAppeared, LiveProcessingStarted, SubscriptionDropped);
    //        _logger?.Debug($"{nameof(BrainUnit)} Subscribed from position: {_catchUpFrom}");
    //    }
    //}

    //private void SubscriptionDropped(EventStoreSubscription arg1, SubscriptionDropReason arg2, Exception arg3)
    //{
    //    SubscribeMe();
    //}

    //private Task EventAppeared(EventStoreSubscription arg1, ResolvedEvent arg2)
    //{
    //    return EventAppearedInternal(arg2);
    //}

    //private void SubscriptionDropped(EventStoreCatchUpSubscription arg1, SubscriptionDropReason arg2,
    //    Exception arg3)
    //{
    //    SubscribeMe();
    //}

    //private void LiveProcessingStarted(EventStoreCatchUpSubscription obj)
    //{
    //    _logger?.Info($"{GetType().FullName} LiveProcessingStarted");
    //}

    //private Task EventAppeared(EventStoreCatchUpSubscription arg1, ResolvedEvent arg2)
    //{
    //    return EventAppearedInternal(arg2);
    //}

    //private Task EventAppearedInternal(ResolvedEvent arg2)
    //{
    //    try
    //    {
    //        if (arg2.Event == null
    //            || arg2.Event.EventStreamId.StartsWith("$")
    //            || arg2.Event.EventType.StartsWith("$") ||
    //            arg2.Event.EventType.StartsWith("Position") ||
    //            arg2.Event.EventType.StartsWith("CloudEvent"))
    //            return Task.CompletedTask;

    //        if (!_handlingFunctions.ContainsKey(arg2.Event.EventType))
    //            return Task.CompletedTask;

    //        var metadataAsJson = Encoding.UTF8.GetString(arg2.Event.Metadata);
    //        if (string.IsNullOrWhiteSpace(metadataAsJson))
    //            return Task.CompletedTask;

    //        var data = JsonNode.Parse(Encoding.UTF8.GetString(arg2.Event.Data));
    //        var metaData = JsonNode.Parse(metadataAsJson);

    //        ProcessEventAppeared(arg2.Event.EventType, data, metaData);
    //        _positionRepository?.Set(arg2.OriginalPosition.Value);
    //    }
    //    catch (Exception e)
    //    {
    //        _logger?.Error(e, e.GetBaseException().Message);
    //    }

    //    return Task.CompletedTask;
    //}

    //internal void ProcessEventAppeared(string eventType, JsonNode data, JsonNode metaData)
    //{
    //    _handlingFunctions[eventType](data, metaData);
    //}

    //private IEventStoreConnection BuildConnection(Uri connectionString, ConnectionSettings connectionSettings, string connectionName)
    //{
    //    var conn = EventStoreConnection.Create(connectionSettings, connectionString, connectionName);
    //    conn.Disconnected += Conn_Disconnected;
    //    conn.Reconnecting += Conn_Reconnecting;

    //    return conn;
    //}

    //private void Conn_Reconnecting(object? sender, ClientReconnectingEventArgs e)
    //{
    //    _logger?.Debug($"Reconnecting to EventStore ConnectionName:'{e.Connection.ConnectionName}'");
    //}

    //private void Conn_Disconnected(object? sender, ClientConnectionEventArgs e)
    //{
    //    _logger?.Error($"Disconnected from EventStore RemoteEndPoint:'{e.RemoteEndPoint}'; ConnectionName:'{e.Connection.ConnectionName}'");
    //}

    //private ConnectionSettings BuildConnectionSettings()
    //{
    //    _logger?.Debug(
    //        $"ConnSettings -> HeartbeatInterval:{_settings.HeartbeatInterval} HeartbeatTimeout:{_settings.HeartbeatTimeout} ReconnectionDelayTo:{_settings.ReconnectionDelayTo}");
    //    var connection = EventStore.ClientAPI.ConnectionSettings.Create()
    //        .SetHeartbeatInterval(TimeSpan.FromSeconds(_settings.HeartbeatInterval))
    //        .SetHeartbeatTimeout(TimeSpan.FromSeconds(_settings.HeartbeatTimeout))
    //        .KeepReconnecting().KeepRetrying()
    //        .SetReconnectionDelayTo(TimeSpan.FromSeconds(_settings.ReconnectionDelayTo));
    //    return connection.Build();
    //}

    #endregion
}
