﻿namespace Evento.Ai.Processor.Domain.Events;

public class BehaviourRequestedV1 : Event
{
    public BehaviourRequestedV1(string area, string tag, string title, string description,
        string acceptanceCriterias, IDictionary<string, string> metadata)
    {
        Area = area;
        Tag = tag;
        Title = title;
        Description = description;
        AcceptanceCriterias = acceptanceCriterias;
        Metadata = metadata;
    }

    public string Area { get; }
    public string Tag { get; }
    public string Title { get; }
    public string Description { get; }
    public string AcceptanceCriterias { get; }
    public IDictionary<string, string> Metadata { get; }
}