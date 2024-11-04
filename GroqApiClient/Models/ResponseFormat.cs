using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class ResponseFormat
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}