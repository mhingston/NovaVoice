using System.Text.Json.Serialization;
using NovaVoice.Speech.TextToSpeech;
using NovaVoice.Tools;

namespace NovaVoice.GroqApiClient.Models;

public abstract class Tool : ITool
{
    [JsonPropertyName("type")]
    public string Type => "function";
    
    [JsonPropertyName("function")]
    public abstract Function Function { get; }

    [JsonIgnore]
    public virtual Func<string, IServiceProvider, ITextToSpeechProvider, Task>? PostExecuteAsync { get; }
}