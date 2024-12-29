namespace Evento.Ai.Processor.Domain.Commands;

public class RequestBehaviour : Command
{
    public string CorrelationId { get; set; }
    public string Area { get; set; }
    public string Tag { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string AcceptanceCriterias { get; set; }
    public IDictionary<string, string> Metadata { get; set; }
}