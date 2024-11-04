using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class Usage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }
    
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
    
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
    
    [JsonPropertyName("prompt_time")]
    public decimal PromptTime { get; set; }
    
    [JsonPropertyName("completion_time")]
    public decimal CompletionTime { get; set; }
    
    [JsonPropertyName("total_time")]
    public decimal TotalTime { get; set; }
}