using System.Text.Json.Serialization;
using NovaVoice.GroqApiClient.Models;
using NovaVoice.Speech.TextToSpeech;

namespace NovaVoice.Tools;

public interface ITool
{
    [JsonPropertyName("type")]
    string Type => "function";
    
    [JsonPropertyName("function")]
    Function Function { get; }
    
    [JsonIgnore]
    Func<string, IServiceProvider, ITextToSpeechProvider, Task>? PostExecuteAsync { get; }
}