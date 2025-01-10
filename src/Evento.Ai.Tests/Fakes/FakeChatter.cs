using Evento.Ai.Processor.Domain.Aggregates.Entities;
using Evento.Ai.Processor.Domain.Services;

namespace Evento.Ai.Tests.Fakes;

public class FakeChatter : IChatter
{
    public dynamic DiscoverSchema(string data)
    {
        return new Schema(Guid.NewGuid().ToString(), "text/json", "{name:'test'}", DateTime.UtcNow, "OpenAi");
    }
}