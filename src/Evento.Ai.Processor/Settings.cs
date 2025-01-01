namespace Evento.Ai.Processor;

public class Settings 
{
    public string OpenAIApiKey { get; set; }
    public string OpenAIOrganization { get; set; }
    public string Api_Link { get; set; }
    public string Api_Token { get; set; }
    public string EventStoreConnectionString { get; set; }
    public string ServiceBusConnectionString { get; set; }
    public string QueueName { get; set; }
    public string EventCategory { get; set; }
    public string CloudRequestSource { get; set; }
}