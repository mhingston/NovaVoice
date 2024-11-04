using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class Message
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("tool_calls")]
    public IEnumerable<ToolCall>? ToolCalls { get; set; }
    
    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
    
}