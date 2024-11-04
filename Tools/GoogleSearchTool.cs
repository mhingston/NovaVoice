using System.Text.Json;
using System.Text.Json.Serialization;
using NovaVoice.GroqApiClient.Models;

namespace NovaVoice.Tools;

public class GoogleSearchTool : Tool
{
    private readonly GoogleApiClient.GoogleApiClient _client;
    public GoogleSearchTool(GoogleApiClient.GoogleApiClient client)
    {
        _client = client;
    }

    [JsonPropertyName("function")]
    public override Function Function => new()
    {
        Description = "Search the web using Google",
        Name = "google_search",
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
        
            var response = await _client.SearchGoogleAsync(query, cancellationToken);

            return new Message
            {
                Content = JsonSerializer.Serialize(response),
                Name = nameof(GoogleSearchTool),
                Role = "tool",
                ToolCallId = id
            };
        }
    };
}