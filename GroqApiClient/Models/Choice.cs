using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class Choice
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    
    [JsonPropertyName("message")]
    public Message Message { get; set; }
    
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }
    
    [JsonPropertyName("logprobs")]
    public bool? Logprobs { get; set; }
}