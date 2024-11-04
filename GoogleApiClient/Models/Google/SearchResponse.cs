using System.Text.Json.Serialization;

namespace NovaVoice.GoogleApiClient.Models.Google;

public class SearchResponse
{
    [JsonPropertyName("items")]
    public List<Item> Items { get; set; }
}