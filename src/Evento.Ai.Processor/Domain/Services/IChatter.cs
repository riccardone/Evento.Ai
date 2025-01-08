using Evento.Ai.Processor.Domain.Aggregates.Entities;

namespace Evento.Ai.Processor.Domain.Services;

public interface IChatter
{
    Schema DiscoverSchema(string data);
}
