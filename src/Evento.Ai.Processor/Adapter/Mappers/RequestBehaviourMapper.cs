using System.Text.Json;
using CloudEventData;
using Evento.Ai.Processor.Domain;
using Evento.Ai.Processor.Domain.Commands;

namespace Evento.Ai.Processor.Adapter.Mappers;

public class RequestBehaviourMapper
{
    public Uri Schema => new Uri("request-behaviour/1.0", UriKind.RelativeOrAbsolute);
    public Uri Source => new Uri("working", UriKind.RelativeOrAbsolute);

    private readonly List<string> _dataContentTypes = new List<string> { "application/json", "application/cloudevents+json" };

    public Command Map(CloudEventRequest request)
    {
        Ensure.NotNull(request, nameof(request));
        if (!_dataContentTypes.Contains(request.DataContentType))
            throw new ArgumentException($"While running Map in '{nameof(RequestBehaviourMapper)}' I can't recognize the DataContentType:{request.DataContentType}");
        if (!request.DataSchema.Equals(Schema) || !request.Source.Equals(Source))
            throw new ArgumentException($"While running Map in '{nameof(RequestBehaviourMapper)}' I can't recognize the data (DataSchema:{request.DataSchema};Source:{request.Source})");
        var cmd = JsonSerializer.Deserialize<RequestBehaviour>(request.Data.ToString());

        cmd.Metadata = new Dictionary<string, string>
        {
            {"$correlationId", cmd.CorrelationId},
            {"source", request.Source.ToString()},
            {"$applies", request.Time.ToString("O")},
            {"cloudrequest-id", request.Id},
            {"schema", request.DataSchema.ToString()},
            {"content-type", request.DataContentType},
            {"command-type", request.Type }
        };
        return cmd;
    }
}