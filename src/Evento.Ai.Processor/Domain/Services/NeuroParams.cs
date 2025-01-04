using Evento.Ai.Processor.Domain.Aggregates.Entities;

namespace Evento.Ai.Processor.Domain.Services;

public class NeuroParams
{
    public NeuroParams(Behaviour behaviour, IDataReader dataReader, IChatter chatter)
    {
        Behaviour = behaviour;
        DataReader = dataReader;
        Chatter = chatter;
    }

    public Behaviour Behaviour { get; }
    public IDataReader DataReader { get; }
    public IChatter Chatter { get; }
}