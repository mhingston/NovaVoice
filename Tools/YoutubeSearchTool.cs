using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using NovaVoice.GoogleApiClient.Models.Youtube;
using NovaVoice.GroqApiClient.Models;
using NovaVoice.Speech.TextToSpeech;

namespace NovaVoice.Tools;

public class YoutubeSearchTool : Tool
{
    private readonly GoogleApiClient.GoogleApiClient _client;
    
    public YoutubeSearchTool(
        GoogleApiClient.GoogleApiClient client)
    {
        _client = client;
    }

    [JsonPropertyName("function")]
    public override Function Function => new()
    {
        Description = "Play music using Youtube Music search",
        Name = "youtube_search",
        Parameters = new Parameters
        {
            Properties = new Dictionary<string, Property>
            {
                {
                    "query", new Property
                    {
                        Type = "string",
                        Description = "The search query"
                    }
                }
            },
            Required = ["query"]
        },
        ExecuteAsync = async (args, cancellationToken) =>
        {
            var id = args["id"].ToString()!;
            var query = args["query"].ToString()!;
        
            var response = await _client.SearchYoutubeAsync(query, cancellationToken);

            return new Message
            {
                Content = JsonSerializer.Serialize(response),
                Name = nameof(YoutubeSearchTool),
                Role = "tool",
                ToolCallId = id
            };
        }
    };
    
    public override Func<string, IServiceProvider, ITextToSpeechProvider, Task>? PostExecuteAsync => 
        async (jsonResult, serviceProvider, textToSpeechProvider) =>
        {
            var response = JsonSerializer.Deserialize<SearchResponse>(jsonResult);
            _ = textToSpeechProvider.SpeakAsync("Playing " + response!.Items.First().Snippet.Title);
            var youtubePlayer = serviceProvider.GetRequiredService<YoutubePlayer>();
            await youtubePlayer.PlayAsync($"https://www.youtube.com/watch?v={response.Items.First().Id.VideoId}");
        };
}