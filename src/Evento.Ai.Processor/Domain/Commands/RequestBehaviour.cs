namespace Evento.Ai.Processor.Domain.Commands;

public class RequestBehaviour : Command
{
    public IDictionary<string, string> Metadata { get; }
}