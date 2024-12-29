namespace Evento.Ai.Processor.Domain.Events;

public class BehaviourRequestedV1 : Event
{
    public IDictionary<string, string> Metadata { get; }
}