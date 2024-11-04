using System.Text.Json.Serialization;

namespace NovaVoice.AccuWeatherApiClient.Models;

public class Headline
{
    [JsonPropertyName("Text")]
    public string Text { get; set; }
}