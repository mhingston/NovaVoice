using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class CreateTranslationRequest
{
    [JsonPropertyName("file")]
    [Required]
    public Stream File { get; set; }

    [JsonIgnore]
    public string FileName { get; set; }

    [JsonPropertyName("model")]
    [Required]
    public string Model { get; set; }

    [JsonPropertyName("prompt")]
    public string? Prompt { get; set; }

    [JsonPropertyName("response_format")]
    public string? ResponseFormat { get; set; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }
}