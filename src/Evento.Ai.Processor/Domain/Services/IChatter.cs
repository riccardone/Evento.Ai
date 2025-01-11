using System.Text.Json;

namespace Evento.Ai.Processor.Domain.Services;

public interface IChatter
{
    JsonDocument DiscoverSchema(string data);
    string DiscoverSchemaName(string data);
}
