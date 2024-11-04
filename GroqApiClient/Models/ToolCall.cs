using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class ToolCall
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("function")]
    public FunctionCall? Function { get; set; }
}