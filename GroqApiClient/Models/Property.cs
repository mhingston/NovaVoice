using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class Property
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("enum")]
    public string[]? Enum { get; set; }
}