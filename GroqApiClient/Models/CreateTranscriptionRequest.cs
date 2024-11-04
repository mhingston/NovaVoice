using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class CreateTranscriptionRequest
{
    [JsonPropertyName("file")]
    [Required]
    public Stream File { get; set; }
    
    [JsonIgnore]
    public string FileName { get; set; }
    
    [JsonPropertyName("model")]
    [Required]
    public string Model { get; set; }
    
    [JsonPropertyName("language")]
    public string? Language { get; set; }
    
    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }
    
    [JsonPropertyName("response_format")]
    public string? ResponseFormat { get; set; }
    
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }
    
    [JsonPropertyName("timestamp_granularities")]
    public string[]? TimestampGranularities { get; set; }
}