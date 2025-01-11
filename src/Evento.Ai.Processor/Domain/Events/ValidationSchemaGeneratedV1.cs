using System.Text.Json;

namespace Evento.Ai.Processor.Domain.Events;

public class ValidationSchemaGeneratedV1 : Event
{
    public ValidationSchemaGeneratedV1(string name, string contentType, JsonDocument content, string provider, IDictionary<string, string> metadata)
    {
        Name = name;
        ContentType = contentType;
        Content = content;
        Metadata = metadata;
        Provider = provider;
    }

    public string Name { get; }
    public string ContentType { get; }
    public JsonDocument Content { get; }
    public string Provider { get; }
    public IDictionary<string, string> Metadata { get; }
}