using System.Text.Json.Serialization;

namespace NovaVoice.GoogleApiClient.Models.Youtube;

public class SearchResponse
{
    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }
}