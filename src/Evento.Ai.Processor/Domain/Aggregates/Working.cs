using Evento.Ai.Processor.Domain.Aggregates.Entities;
using Evento.Ai.Processor.Domain.Commands;
using Evento.Ai.Processor.Domain.Events;
using Evento.Ai.Processor.Domain.Services;

namespace Evento.Ai.Processor.Domain.Aggregates;

public class Working : AggregateBase
{
    public override string AggregateId => _correlationId ?? "undefined";
    private string? _correlationId;
    private Behaviour? _behaviour;
    private readonly IDictionary<string, Neuron> _neurons;

    public Working()
    {
        RegisterTransition<BehaviourRequestedV1>(Apply);
        _neurons = CreateNeurons();
    }

    private void Apply(BehaviourRequestedV1 evt)
    {
        _correlationId = evt.Metadata["$correlationId"];
        _behaviour = new Behaviour(evt.Area, evt.Tag, evt.Title, evt.Description, evt.AcceptanceCriterias);
    }

    public void RequestBehaviour(RequestBehaviour command, IDataReader reader, IChatter chatter)
    {
        Ensure.NotNull(command, nameof(command));
        Ensure.NotNull(command.CorrelationId, nameof(command.CorrelationId));

        if (_behaviour != null) return;

        if (_neurons.ContainsKey("ValidationSchema"))
        {
            _neurons["ValidationSchema"]
                .Handle(new NeuroParams(
                    new Behaviour(command.Area, command.Tag, command.Title, command.Description,
                        command.AcceptanceCriterias), reader, chatter));
        }

        RaiseEvent(new BehaviourRequestedV1(command.Area, command.Tag, command.Title, command.Description, command.AcceptanceCriterias, command.Metadata));
    }

    public static Working Create()
    {
        return new Working();
    }

    private IDictionary<string, Neuron> CreateNeurons()
    {
        return new Dictionary<string, Neuron>
        {
            {"ValidationSchema", new ValidationSchemaGenerator()}
        };
    }
}