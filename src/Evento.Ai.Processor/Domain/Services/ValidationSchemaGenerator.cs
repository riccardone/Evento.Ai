namespace Evento.Ai.Processor.Domain.Services;

public class ValidationSchemaGenerator : Neuron
{
    public override void Handle(NeuroParams p)
    {
        var schema = p.Chatter.DiscoverSchema(p.Behaviour);
        // TODO raise an event that contains the schema?
        throw new NotImplementedException();
    }
}