using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NovaVoice.Tools;

namespace NovaVoice.GroqApiClient.Models;

public class CreateChatCompletionRequest
{
    [JsonPropertyName("messages")]
    [Required]
    public List<Message> Messages { get; set; }
    
    [JsonPropertyName("model")]
    [Required]
    public string Model { get; set; }
    
    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }
    
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }
    
    [JsonPropertyName("n")]
    public int? N { get; set; }
    
    [JsonPropertyName("parallel_tool_calls")]
    public bool? ParallelToolCalls { get; set; }
    
    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }
    
    [JsonPropertyName("response_format")]
    public ResponseFormat? ResponseFormat { get; set; }
    
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }
    
    [JsonPropertyName("stop")]
    public string[]? Stop { get; set; }
    
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }
    
    [JsonPropertyName("tool_choice")]
    public string? ToolChoice { get; set; }
    
    [JsonPropertyName("tools")]
    public IEnumerable<ITool>? Tools { get; set; }
    
    [JsonPropertyName("top_logprobs")]
    public int? TopLogProbs { get; set; }
    
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }
    
    [JsonPropertyName("user")]
    public string? User { get; set; }
}