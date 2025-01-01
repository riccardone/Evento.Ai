using Evento.Ai.Processor;
using EventStore.ClientAPI;
using ILogger = NLog.ILogger;

namespace Evento.Ai.Host;

public class ConnectionBuilder : IConnectionBuilder
{
    private readonly ILogger _logger;
    public Uri ConnectionString { get; }
    public ConnectionSettings ConnectionSettings { get; }
    public string ConnectionName { get; }

    public IEventStoreConnection Build(bool openConnection = true)
    {
        var conn = EventStoreConnection.Create(ConnectionSettings, ConnectionString, ConnectionName);
        conn.Disconnected += Conn_Disconnected;
        conn.Reconnecting += Conn_Reconnecting;
        conn.Connected += Conn_Connected;
        conn.Closed += Conn_Closed;
        if (openConnection)
            conn.ConnectAsync().Wait();

        return conn;
    }

    private void Conn_Connected(object sender, ClientConnectionEventArgs e)
    {
        _logger.Debug($"Connected to EventStore RemoteEndPoint:'{e.RemoteEndPoint}';ConnectionName:'{e.Connection.ConnectionName}'");
    }

    private void Conn_Reconnecting(object sender, ClientReconnectingEventArgs e)
    {            
        _logger.Debug($"Reconnecting to EventStore ConnectionName:'{e.Connection.ConnectionName}'");
    }

    private void Conn_Disconnected(object sender, ClientConnectionEventArgs e)
    {
        _logger.Debug($"Disconnected from EventStore RemoteEndPoint:'{e.RemoteEndPoint}';ConnectionName:'{e.Connection.ConnectionName}'");
    }

    private void Conn_Closed(object sender, ClientClosedEventArgs e)
    {
        _logger.Debug($"Disconnected from EventStore RemoteEndPoint:'{e.Reason}';ConnectionName:'{e.Connection.ConnectionName}'");
    }

    public ConnectionBuilder(Uri connectionString, ConnectionSettings connectionSettings, string connectionName, ILogger logger)
    {
        ConnectionString = connectionString;
        ConnectionSettings = connectionSettings;
        ConnectionName = connectionName;
        _logger = logger;
    }

    public static ConnectionSettings BuildConnectionSettings(Settings settings)
    {
        var connectionSettingsBuilder = ConnectionSettings.Create();
        //.KeepReconnecting().KeepRetrying();
        return connectionSettingsBuilder.Build();
    }
}