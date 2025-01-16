using Evento.Ai.Processor.Domain.Aggregates.Entities;

namespace Evento.Ai.Processor.Domain.Services;

public class ChatterService
{
    private readonly IChatter _chatter;

    public ChatterService(IChatter chatter)
    {
        _chatter = chatter;
    }

    public Schema GetValidationSchema(string data)
    {
        var schema = _chatter.DiscoverSchema(data);
        var schemaName = _chatter.DiscoverSchemaName(data);
        return new Schema(schemaName, "application/json", schema, DateTime.UtcNow, nameof(_chatter));
    }
}