using Evento.Ai.Processor.Domain.Aggregates.Entities;
using Evento.Ai.Processor.Domain.Services;
using OpenAI.Chat;
using OpenAI.Models;

namespace Evento.Ai.Chatter;

public class OpenAiChatter : IChatter
{
    private readonly Model _model = Model.GPT4;

    public Schema DiscoverSchema(string data)
    {
        var chatRequest = new ChatRequest(ModelTrainer.DiscoverInfoAboutValidationSchema(data), _model);
        return new Schema(Guid.NewGuid().ToString(), "text/json", "{name:'test'}", DateTime.UtcNow, "OpenAi");
    }
}