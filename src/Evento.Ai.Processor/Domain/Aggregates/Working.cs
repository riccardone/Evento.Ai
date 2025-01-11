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
    private readonly IDictionary<string, Schema> _validationSchemas = new Dictionary<string, Schema>();

    public Working()
    {
        RegisterTransition<BehaviourRequestedV1>(Apply);
        RegisterTransition<ValidationSchemaGeneratedV1>(Apply);
    }

    private void Apply(ValidationSchemaGeneratedV1 evt)
    {
        _correlationId = evt.Metadata["$correlationId"];
        _validationSchemas[evt.Name] = new Schema(evt.Name, evt.ContentType, evt.Content,
            DateTime.Parse(evt.Metadata["$applies"]), evt.Provider);
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
        Ensure.NotNull(reader, nameof(reader));
        Ensure.NotNull(chatter, nameof(chatter));

        var schema = chatter.DiscoverSchema(command.Description);
        var schemaName = chatter.DiscoverSchemaName(command.Description);
        if (!_validationSchemas.ContainsKey(command.CorrelationId))
            RaiseEvent(new ValidationSchemaGeneratedV1(schemaName, "application/json", schema, nameof(chatter), command.Metadata));
        if (_behaviour == null)
            RaiseEvent(new BehaviourRequestedV1(command.Area, command.Tag, command.Title, command.Description, command.AcceptanceCriterias, command.Metadata));
    }

    public static Working Create()
    {
        return new Working();
    }
}