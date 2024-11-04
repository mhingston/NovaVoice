using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class Parameters
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "object";
    
    [JsonPropertyName("properties")]
    public Dictionary<string, Property>? Properties { get; set; }
    
    [JsonPropertyName("required")]
    public string[]? Required { get; set; }
}