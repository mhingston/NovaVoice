using System.Text.Json.Serialization;

namespace NovaVoice.AccuWeatherApiClient.Models;

public class WeatherResponse
{
    [JsonPropertyName("Headline")]
    public Headline Headline { get; set; }
}