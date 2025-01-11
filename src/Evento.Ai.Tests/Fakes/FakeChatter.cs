using Evento.Ai.Processor.Domain.Aggregates.Entities;
using Evento.Ai.Processor.Domain.Services;
using System.Text.Json;

namespace Evento.Ai.Tests.Fakes;

public class FakeChatter : IChatter
{
    public JsonDocument DiscoverSchema(string data)
    {
        return JsonSerializer.SerializeToDocument(new Schema(Guid.NewGuid().ToString(), "text/json",
            JsonDocument.Parse("{name:'test'}"), DateTime.UtcNow, "OpenAi"));
    }

    public string DiscoverSchemaName(string data)
    {
        return "accept-invitation";
    }
}