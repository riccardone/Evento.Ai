using OpenAI;

namespace Evento.Ai.Chatter;

public class ModelTrainer
{
    public static IEnumerable<OpenAI.Chat.Message> DiscoverInfoAboutValidationSchema(string data)
    {
        return new List<OpenAI.Chat.Message>
        {
            new(Role.System, @"
In the context of a distributed application where incoming messages need to be validated, you are going to create a json schema based on a user story and some acceptance criteria. You need to define in the schema all the properties that are necessary to implement the requirement. Try to use flat properties instead of hierarchies if possible. Each property has a data type and some constraints to limit security risks. You will need to create a JSON response with the Json schema. Each schema must have a CorrelationId string property and TenantId string property. These two mandatory properties have minLength=3 and maxLength=256. In the schema, additionalProperties is set to false.
User: As a interested user I want to accept an invitation So I can join the company on the platform
Assistant: {
    ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
    ""type"": ""object"",
    ""properties"": {
        ""CorrelationId"": {
            ""type"": ""string"",
            ""minLength"": 3,
            ""maxLength"": 256
        },
        ""UserId"": {
            ""type"": ""string"",
            ""minLength"": 3,
            ""maxLength"": 256
        },
        ""TenantId"": {
            ""type"": ""string""
            """"minLength"""": 3,
            """"maxLength"""": 256
        }
    },
    ""required"": [
        ""CorrelationId"",
        ""UserId"",
        ""TenantId""
    ],
    ""additionalProperties"": false
}
"),
            new(Role.User, data)
        };
    }

    public static IEnumerable<OpenAI.Chat.Message> DiscoverSchemaName(string data)
    {
        return new List<OpenAI.Chat.Message>
        {
            new(Role.System, @"
In the context of a distributed application where incoming messages need to be validated with a Json Schema, you are going to define the name of that schema related to a user story. Use words in lower case separated with a dash and be brief. Only return the name of the schema you think it's best.
User: As a interested user I want to accept an invitation So I can join the company on the platform
Assistant: accept-invitation
"),
            new(Role.User, data)
        };
    }
}