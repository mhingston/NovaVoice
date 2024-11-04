using System.Text.Json;
using System.Text.Json.Serialization;
using NovaVoice.GroqApiClient.Models;

namespace NovaVoice.Tools;

public class WeatherTool : Tool
{
    private readonly AccuWeatherApiClient.AccuWeatherApiClient _client;
    public WeatherTool(AccuWeatherApiClient.AccuWeatherApiClient client)
    {
        _client = client;
    }

    [JsonPropertyName("function")]
    public override Function Function => new()
    {
        Description = "Check the weather in a location",
        Name = "check_weather",
        Parameters = new Parameters
        {
            Properties = new Dictionary<string, Property>
            {
                {
                    "location", new Property
                    {
                        Type = "string",
                        Description = "The location to check the weather for"
                    }
                }
            },
            Required = ["location"]
        },
        ExecuteAsync = async (args, cancellationToken) =>
        {
            var id = args["id"].ToString()!;
            var query = args["location"].ToString()!;
        
            var locationResponses = await _client.GetLocationKeyAsync(query, cancellationToken);
            var location = locationResponses.First();
            var weatherResponse = await _client.GetWeatherAsync(location.Key, cancellationToken);

            return new Message
            {
                Content = JsonSerializer.Serialize(weatherResponse),
                Name = nameof(WeatherTool),
                Role = "tool",
                ToolCallId = id
            };
        }
    };
}