using Evento.Ai.Processor.Domain.Aggregates.Entities;
using Evento.Ai.Processor.Domain.Services;

namespace Evento.Ai.Tests.Fakes;

public class FakeChatter : IChatter
{
    public Schema DiscoverSchema(Behaviour behaviour)
    {
        throw new NotImplementedException();
    }
}