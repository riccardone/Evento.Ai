using Evento.Ai.Processor.Domain.Services;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Evento.Ai.Chatter;

public class OpenAiChatter : IChatter
{
    private readonly OpenAIClient _openAiClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Model _model;

    public OpenAiChatter(OpenAIClient openAiClient)
    {
        _openAiClient = openAiClient;
        _model = Model.GPT4;
        _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public JsonDocument DiscoverSchema(string data)
    {
        var chatRequest = new ChatRequest(ModelTrainer.DiscoverInfoAboutValidationSchema(data), _model);
        var result = _openAiClient.ChatEndpoint.GetCompletionAsync(chatRequest).Result;
        return JsonSerializer.Deserialize<JsonDocument>(result.ToString(), _jsonOptions);
    }

    public string DiscoverSchemaName(string data)
    {
        var chatRequest = new ChatRequest(ModelTrainer.DiscoverSchemaName(data), _model);
        var result = _openAiClient.ChatEndpoint.GetCompletionAsync(chatRequest).Result;
        return result.FirstChoice.Message.ToString();
    }
}