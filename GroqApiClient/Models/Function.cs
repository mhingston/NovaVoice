using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class Function
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("parameters")]
    public Parameters? Parameters { get; set; }
    
    [JsonIgnore]
    public Func<Dictionary<string, object>, CancellationToken, Task<Message>> ExecuteAsync { get; set; }
}