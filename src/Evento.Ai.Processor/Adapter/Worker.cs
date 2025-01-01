using System.Collections.ObjectModel;
using System.Text;
using CloudEventData;
using Evento.Ai.Processor.Domain.Commands;
using NLog;

namespace Evento.Ai.Processor.Adapter;

public class Worker :
    IHandle<RequestBehaviour>
{
    private readonly IDomainRepository _domainRepository;
    private readonly ILogger _logger;

    private readonly Dictionary<string, Func<CloudEventRequest, Command>> _deserializers = CreateDeserializersMapping();

    public Worker(IDomainRepository domainRepository,
        ILogger logger)
    {
        _domainRepository = domainRepository;
        _logger = logger;
    }

    public void Process(CloudEventRequest cloudRequest)
    {
        if (!_deserializers.ContainsKey(cloudRequest.DataSchema.ToString()) &&
            !_deserializers.ContainsKey($"{cloudRequest.DataSchema}{cloudRequest.Source}"))
            throw new Exception(
                $"I can't find a mapper for schema:'{cloudRequest.DataSchema}' source:''{cloudRequest.Source}''");

        var command = _deserializers.ContainsKey(cloudRequest.DataSchema.ToString())
            ? _deserializers[cloudRequest.DataSchema.ToString()](cloudRequest)
            : _deserializers[$"{cloudRequest.DataSchema}{cloudRequest.Source}"](cloudRequest);

        if (command == null)
            throw new Exception(
                $"I received CloudRequest Type:'{cloudRequest.Type}' Source:'{cloudRequest.Source}' Schema:'{cloudRequest.DataSchema}' but I was unable to deserialize a Command out of it");

        if (command.Metadata.Count > 0)
        {
            ScopeContext.Clear();
            var metadataToLog = new ReadOnlyDictionary<string, string>(command.Metadata.ToDictionary(x => x.Key.Replace("$", ""), x => x.Value));
            ScopeContext.PushProperties(metadataToLog);
        }

        IAggregate? aggregate = null;
        try
        {
            switch (command)
            {
                case RequestBehaviour requestBehaviour:
                    aggregate = Handle(requestBehaviour);
                    break;
            }

            // Add here any further command matches

            if (aggregate == null)
                throw new Exception(
                    $"Received CloudRequest Type:'{cloudRequest.Type}' Source:'{cloudRequest.Source}' Schema:'{cloudRequest.DataSchema}' but I can't find an available handler for it");
        }
        finally
        {
            if (aggregate != null && aggregate.UncommitedEvents().Any())
            {
                var uncommittedEventsList = aggregate.UncommitedEvents().ToList();
                _domainRepository.Save(aggregate);

                var error = new StringBuilder();
                foreach (var uncommittedEvent in uncommittedEventsList)
                {
                    _logger.Info(
                        $"Handled '{cloudRequest.Type}' AggregateId:'{aggregate.AggregateId}' [0]Resulted event:'{uncommittedEvent.GetType()}'");

                    if (uncommittedEvent.GetType().ToString().EndsWith("FailedV1"))
                    {
                        error.Append(HandleFailedEvent(uncommittedEvent, command));
                    }
                }

                if (error.Length > 0)
                {
                    // TODO do we need our own BusinessException?
                    throw new Exception(error.ToString());
                }
            }
            else
                _logger.Info(
                    $"Handled CloudRequest Type:'{cloudRequest.Type}' Source:'{cloudRequest.Source}' Schema:'{cloudRequest.DataSchema}' with no events to save");
        }
    }

    private string HandleFailedEvent(Event failedEvent, Command command)
    {
        var errMessage = string.Empty;
        var errForLogging = string.Empty;
        if (failedEvent.GetType().ToString().EndsWith("FailedV1"))
        {
            errMessage = !failedEvent.Metadata.ContainsKey("error")
                ? $"Error while processing a '{command.Metadata["source"]}' (no error message has been set in command metadata)"
                : $"Error while processing a '{command.Metadata["source"]}' contracting: {failedEvent.Metadata["error"]}";
            errForLogging = failedEvent.Metadata.ContainsKey("error") ? failedEvent.Metadata["error"] : "undefined";
        }
        // any other business error
        // if (failedEvent.GetType().ToString().EndsWith("...FailedV1"))

        var errStack = !failedEvent.Metadata.ContainsKey("error-stack")
            ? string.Empty
            : $"StackTrace: {failedEvent.Metadata["error-stack"]}";
        var err = $"{errMessage} - {errStack}";
        var correlationId = failedEvent.Metadata.ContainsKey("$correlationId")
            ? failedEvent.Metadata["$correlationId"]
            : "undefined";

        var msgToLog = $"CorrelationId:'{correlationId}';{errForLogging}";
        _logger.Error(msgToLog);
        return err;
    }

    private static Dictionary<string, Func<CloudEventRequest, Command>> CreateDeserializersMapping() 
    {
        var mappers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
            .Where(t => t.IsClass 
                && !t.IsAbstract 
                && t.Namespace is not null 
                && t.Namespace.Contains("Adapter.Mappers", 
                StringComparison.Ordinal))
            .ToList(); 
        var deserialisers = new Dictionary<string, Func<CloudEventRequest, Command>>(); 
        foreach (var mapper in mappers) 
        { 
            var instance = Activator.CreateInstance(mapper); 
            var schemaField = mapper?.GetProperty("Schema")?.GetValue(instance)?.ToString(); 
            var methodInfo = mapper.GetMethod("Map"); 
            Command func(CloudEventRequest request) => (Command)methodInfo.Invoke(instance, new object[] { request }); 
            deserialisers.Add(schemaField, func); 
        } 
        return deserialisers; 
    }

    public IAggregate Handle(RequestBehaviour command)
    {
        Domain.Aggregates.Working aggregate;

        try
        {
            aggregate = _domainRepository.GetById<Domain.Aggregates.Working>(command.Metadata["$correlationId"]);
        }
        catch (AggregateNotFoundException)
        {
            aggregate = Domain.Aggregates.Working.Create();
        }

        aggregate.RequestBehaviour(command);

        return aggregate;
    }
}
