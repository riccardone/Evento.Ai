namespace Evento.Ai.Tests.Fakes
{
    public class InMemoryDomainRepository : IDomainRepository
    {
        private readonly Dictionary<string, List<Event>> _eventStore = new();
        //private IAggregate _aggregate;
        private IDictionary<Type, IAggregate> _aggregates = new Dictionary<Type, IAggregate>();

        public Dictionary<string, List<Event>> EventStore => _eventStore;
        public IEnumerable<Event> Save<TAggregate>(TAggregate aggregate) where TAggregate : IAggregate
        {
            var uncommittedEvents = aggregate.UncommitedEvents().ToList();
            if (!_eventStore.ContainsKey(aggregate.AggregateId))
            {
                _eventStore.Add(aggregate.AggregateId, aggregate.UncommitedEvents().ToList());
            }
            else
            {
                _eventStore[aggregate.AggregateId].AddRange(aggregate.UncommitedEvents().ToList());
            }
            aggregate.ClearUncommitedEvents();
            if (!_aggregates.ContainsKey(aggregate.GetType()))
                _aggregates.Add(aggregate.GetType(), aggregate);
            _aggregates[aggregate.GetType()] = aggregate;
            return uncommittedEvents;
        }

        public Task<IEnumerable<Event>> SaveAsync<TAggregate>(TAggregate aggregate) where TAggregate : IAggregate
        {
            var uncommittedEvents = aggregate.UncommitedEvents().ToList();
            if (!_eventStore.ContainsKey(aggregate.AggregateId))
            {
                _eventStore.Add(aggregate.AggregateId, aggregate.UncommitedEvents().ToList());
            }
            else
            {
                _eventStore[aggregate.AggregateId].AddRange(aggregate.UncommitedEvents().ToList());
            }
            aggregate.ClearUncommitedEvents();
            _aggregates[aggregate.GetType()] = aggregate;
            return new Task<IEnumerable<Event>>(uncommittedEvents.AsEnumerable);
        }

        public TResult GetById<TResult>(string correlationId) where TResult : IAggregate, new()
        {
            foreach (var aggregate in _aggregates)
            {
                try
                {
                    var result = (TResult) aggregate.Value;
                    if (result.AggregateId.EndsWith(correlationId))
                        return result;
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
            //if (_aggregates.ContainsKey(TResult) != null && _aggregate.AggregateId.EndsWith(correlationId))
            //    return (TResult)_aggregate;
            throw new AggregateNotFoundException("inmemory");
        }

        public TResult GetById<TResult>(string correlationId, int eventsToLoad) where TResult : IAggregate, new()
        {
            foreach (var aggregate in _aggregates)
            {
                try
                {
                    var result = (TResult)aggregate.Value;
                    if (result.AggregateId.EndsWith(correlationId))
                        return result;
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
            //if (_aggregate != null && _aggregate.AggregateId.EndsWith(correlationId))
            //    return (TResult)_aggregate;
            throw new AggregateNotFoundException("inmemory");
        }

        public void DeleteAggregate<TAggregate>(string correlationId, bool hard)
        {

        }
    }
}
