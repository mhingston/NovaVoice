using System.Text.Json.Serialization;

namespace NovaVoice.GroqApiClient.Models;

public class ListModelsResponse
{
    [JsonPropertyName("object")]
    public string Object { get; set; }
    
    [JsonPropertyName("data")]
    public IEnumerable<GetModelResponse> Data { get; set; }

}