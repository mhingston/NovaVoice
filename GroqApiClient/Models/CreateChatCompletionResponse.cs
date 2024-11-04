using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class CreateChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("object")]
    public string Object { get; set; }
    
    [JsonPropertyName("created")]
    [JsonConverter(typeof(UnixEpochDateTimeJsonConverter))]
    public DateTime Created { get; set; }
    
    [JsonPropertyName("model")]
    public string Model { get; set; }
    
    [JsonPropertyName("system_fingerprint")]
    public string SystemFingerprint { get; set; }
    
    public List<Choice> Choices { get; set; }
    
    [JsonPropertyName("usage")]
    public Usage Usage { get; set; }
}