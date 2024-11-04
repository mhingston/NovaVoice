using System.Text.Json.Serialization;

namespace NovaVoice.GoogleApiClient.Models.Youtube;

public class Snippet
{
    [JsonPropertyName("publishedAt")]
    public DateTime PublishedAt { get; set; }
    
    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    
    [JsonPropertyName("channelTitle")]
    public string ChannelTitle { get; set; }
    
    [JsonPropertyName("liveBroadcastContent")]
    public string LiveBroadcastContent { get; set; }
    
    [JsonPropertyName("publishTime")]
    public DateTime PublishTime { get; set; }
}