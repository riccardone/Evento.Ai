using Evento.Ai.Processor.Adapter;
using Evento.Ai.Processor.Domain.Events;
using Evento.Ai.Tests.Fakes;
using NLog;

namespace Evento.Ai.Tests;

public class WorkingTests
{
    private Worker _sut;
    private InMemoryDomainRepository _repo;
    private NLog.ILogger _logger;

    [SetUp]
    public void Setup()
    {
        _logger = new NLog.NullLogger(new LogFactory());
        _repo = new InMemoryDomainRepository();
        _sut = new Worker(_repo, new FakeDataReader(),new FakeChatter(), _logger);
    }

    [Test]
    public void Given_RequestBehaviour_Then_I_Expect_BehaviourRequested()
    {
        // Assign
        var requestBehaviour = Helpers.BuildCloudRequest("./PayloadSamples/request-behaviour-1_0.json");

        // Act
        _sut.Process(requestBehaviour);

        // Assert
        Assert.True(_repo.EventStore.Single().Value[0] is BehaviourRequestedV1);
    }
}