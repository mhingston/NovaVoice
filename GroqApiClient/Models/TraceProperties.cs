using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class TraceProperties
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
}