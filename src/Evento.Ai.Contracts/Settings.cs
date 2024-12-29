namespace Evento.Ai.Contracts;

public class Settings 
{
    public string OpenAIApiKey { get; set; }
    public string OpenAIOrganization { get; set; }
    public string EventStoreConnectionString { get; set; }
    public string Api_Link { get; set; }
    public string Api_Token { get; set; }
    public bool Volatile { get; set; }
}