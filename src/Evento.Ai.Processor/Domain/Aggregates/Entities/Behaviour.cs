namespace Evento.Ai.Processor.Domain.Aggregates.Entities;

public class Behaviour
{
    public Behaviour(string area, string tag, string title, string description, string acceptanceCriterias)
    {
        Area = area;
        Tag = tag;
        Title = title;
        Description = description;
        AcceptanceCriterias = acceptanceCriterias;
    }

    public string Area { get; }
    public string Tag { get; }
    public string Title { get; }
    public string Description { get; }
    public string AcceptanceCriterias { get; }
}