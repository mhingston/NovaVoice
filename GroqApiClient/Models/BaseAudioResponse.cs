using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public abstract class BaseAudioResponse
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
    
    [JsonPropertyName("x_groq")]
    public TraceProperties XGroq { get; set; }
}