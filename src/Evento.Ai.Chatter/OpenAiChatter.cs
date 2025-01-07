using Evento.Ai.Processor.Domain.Aggregates.Entities;
using Evento.Ai.Processor.Domain.Services;

namespace Evento.Ai.Chatter;

public class OpenAiChatter : IChatter
{
    public Schema DiscoverSchema(Behaviour behaviour)
    {
        return new Schema(Guid.NewGuid().ToString(), "text/json", "{name:'test'}", DateTime.UtcNow, "OpenAi");
    }
}