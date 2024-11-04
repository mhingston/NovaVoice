using System.Net.Http.Json;
using System.Net.Mime;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NovaVoice.Models;

namespace NovaVoice.GoogleApiClient;

public class GoogleApiClient : IGoogleApiClient
{
    private readonly HttpClient _httpClient;
    private readonly Configuration _configuration;

    public GoogleApiClient(HttpClient httpClient, IOptions<Configuration> configuration)
    {
        _configuration = configuration.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://www.googleapis.com/");
        _httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Accept, MediaTypeNames.Application.Json);
    }
    
    public async Task<Models.Google.SearchResponse> SearchGoogleAsync(string query, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"customsearch/v1?key={_configuration.Google.ApiKey}&cx={_configuration.Google.CustomSearchId}&q={query}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var searchResults = await response.Content.ReadFromJsonAsync<Models.Google.SearchResponse>(cancellationToken: cancellationToken);
        return searchResults!;
    }
    
    public async Task<Models.Youtube.SearchResponse> SearchYoutubeAsync(string query, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"youtube/v3/search?part=snippet&q={query}&key={_configuration.Google.ApiKey}&type=video,playlist", cancellationToken);
        response.EnsureSuccessStatusCode();
        var searchResults = await response.Content.ReadFromJsonAsync<Models.Youtube.SearchResponse>(cancellationToken: cancellationToken);
        return searchResults!;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}