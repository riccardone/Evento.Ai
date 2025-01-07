namespace Evento.Ai.Processor.Domain.Aggregates.Entities;

public class Schema
{
    public Schema(string id, string contentType, string data, DateTime createdAt, string provider)
    {
        Id = id;
        ContentType = contentType;
        Data = data;
        CreatedAt = createdAt;
        Provider = provider;
    }

    public string Id { get; }
    public string ContentType { get; }
    public string Data { get; }
    public DateTime CreatedAt { get; }
    public string Provider { get; }
}