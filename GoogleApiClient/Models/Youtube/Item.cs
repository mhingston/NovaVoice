using System.Text.Json.Serialization;

namespace NovaVoice.GoogleApiClient.Models.Youtube;

public class Item
{
    [JsonPropertyName("id")]
    public Id Id { get; set; }
    
    [JsonPropertyName("snippet")]
    public Snippet Snippet { get; set; }
}