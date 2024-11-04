using System.Text.Json.Serialization;

namespace NovaVoice.AccuWeatherApiClient.Models;

public class LocationResponse
{
    [JsonPropertyName("Key")]
    public string Key { get; set; }
    
    [JsonPropertyName("EnglishName")]
    public string EnglishName { get; set; }
}