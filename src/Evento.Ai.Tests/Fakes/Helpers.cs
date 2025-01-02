using System.Text.Json;
using System.Text.Json.Nodes;
using CloudEventData;
using Newtonsoft.Json.Linq;

namespace Evento.Ai.Tests.Fakes;

public static class Helpers
{
    public static CloudEventRequest BuildCloudRequest(string payloadPath)
    {
        var request = File.ReadAllText(payloadPath);
        var req = JsonSerializer.Deserialize<CloudEventRequest>(request, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        req.Data = req.Data.ToString();
        return req;
    }

    public static CloudEventRequest BuildCloudRequest(string payloadPath, Dictionary<string, dynamic> newvalues)
    {
        var req = BuildCloudRequest(payloadPath);
        return EditCloudRequest(req, newvalues);
    }

    public static CloudEventRequest BuildCloudRequest(string payloadPath, DateTime cloudEventTime)
    {
        var request = BuildCloudRequest(payloadPath);
        request.Time = cloudEventTime;
        return request;
    }

    public static CloudEventRequest BuildCloudRequest(string payloadPath, Dictionary<string, dynamic> newvalues, DateTime cloudEventTime)
    {
        var request = BuildCloudRequest(payloadPath);
        request.Time = cloudEventTime;
        return EditCloudRequest(request, newvalues);
    }

    public static CloudEventRequest BuildCloudRequest(JsonObject json)
    {
        return JsonSerializer.Deserialize<CloudEventRequest>(json.ToJsonString());
    }

    public static T GetCommand<T>(JsonElement json) where T : Command
    {
        return json.Deserialize<T>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public static T GetCommand<T>(string json) where T : Command
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    public static CloudEventRequest EditCloudRequest(CloudEventRequest request, Dictionary<string, dynamic> newvalues)
    {
        JObject data = JObject.Parse(request.Data);

        foreach (var newvalue in newvalues)
        {
            data[newvalue.Key] = newvalue.Value;
        }
        request.Data = data.ToString();
        return request;
    }

    public static bool HasPropertyWithValue(this JsonObject jsonObject, string propertyName, string expectedValue)
    {
        return jsonObject.TryGetPropertyValue(propertyName, out var value)
            && value.GetValue<string>() == expectedValue;
    }
}