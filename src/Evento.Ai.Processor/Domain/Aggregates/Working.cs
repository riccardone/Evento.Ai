using Evento.Ai.Processor.Domain.Aggregates.Entities;
using Evento.Ai.Processor.Domain.Commands;
using Evento.Ai.Processor.Domain.Events;

namespace Evento.Ai.Processor.Domain.Aggregates;

public class Working : AggregateBase
{
    public override string AggregateId => _correlationId ?? "undefined";
    private string? _correlationId;
    private Behaviour? _behaviour;

    public Working()
    {
        RegisterTransition<BehaviourRequestedV1>(Apply);
    }

    private void Apply(BehaviourRequestedV1 evt)
    {
        _correlationId = evt.Metadata["$correlationId"];
        _behaviour = new Behaviour(evt.Area, evt.Tag, evt.Title, evt.Description, evt.AcceptanceCriterias);
    }

    public void RequestBehaviour(RequestBehaviour command)
    {
        Ensure.NotNull(command, nameof(command));
        Ensure.NotNull(command.CorrelationId, nameof(command.CorrelationId));

        if (_behaviour != null) return;

        RaiseEvent(new BehaviourRequestedV1(command.Area, command.Tag, command.Title, command.Description, command.AcceptanceCriterias, command.Metadata));
    }

    public static Working Create()
    {
        return new Working();
    }
}