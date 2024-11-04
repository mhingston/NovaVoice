using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class GetModelResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("object")]
    public string Object { get; set; }
    
    [JsonPropertyName("created")]
    [JsonConverter(typeof(UnixEpochDateTimeJsonConverter))]
    public DateTime Created { get; set; }
    
    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; }
    
    [JsonPropertyName("context_window")]
    public int ContextWindow { get; set; }
}