using Evento.Ai.Chatter;
using Evento.Ai.Processor;
using Evento.Ai.Processor.Adapter;
using Evento.Ai.Processor.Domain.Events;
using Evento.Ai.Tests.Fakes;
using Microsoft.Extensions.Configuration;
using NLog;
using OpenAI;

namespace Evento.Ai.Tests;

public class WorkingTests
{
    private Worker _sut;
    private InMemoryDomainRepository _repo;
    private NLog.ILogger _logger;
    private Settings _settings;

    [OneTimeSetUp]
    public void Initialise()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "dev";
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();
        _settings = configuration.Get<Settings>();
    }

    [SetUp]
    public void Setup()
    {
        _logger = new NLog.NullLogger(new LogFactory());
        _repo = new InMemoryDomainRepository();
        _sut = new Worker(_repo, new FakeDataReader(), new FakeChatter(), _logger);
    }

    [Test]
    public void Given_RequestBehaviour_Then_I_Expect_BehaviourRequested()
    {
        // Assign
        var sut = new Worker(_repo, new FakeDataReader(),
            new OpenAiChatter(
                new OpenAIClient(new OpenAIAuthentication(_settings.OpenAIApiKey, _settings.OpenAIOrganization))),
            _logger);
        var requestBehaviour = Helpers.BuildCloudRequest("./PayloadSamples/request-behaviour-1_0.json");

        // Act
        sut.Process(requestBehaviour);

        // Assert
        Assert.True(_repo.EventStore.Single().Value[0] is BehaviourRequestedV1);
    }
}