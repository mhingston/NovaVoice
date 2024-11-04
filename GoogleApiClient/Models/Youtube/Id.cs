using System.Text.Json.Serialization;

namespace NovaVoice.GoogleApiClient.Models.Youtube;

public class Id
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; }
    
    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; }
    
    [JsonPropertyName("videoId")]
    public string VideoId { get; set; }
}