namespace NovaVoice.GoogleApiClient;

public interface IGoogleApiClient : IDisposable
{
    Task<Models.Google.SearchResponse> SearchGoogleAsync(string query, CancellationToken cancellationToken = default);
    Task<Models.Youtube.SearchResponse> SearchYoutubeAsync(string query, CancellationToken cancellationToken = default);
}