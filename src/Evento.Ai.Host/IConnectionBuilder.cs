using EventStore.ClientAPI;

namespace Evento.Ai.Host;

public interface IConnectionBuilder
{
    IEventStoreConnection Build(bool openConnection = true);
}