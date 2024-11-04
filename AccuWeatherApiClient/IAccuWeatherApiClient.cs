using NovaVoice.AccuWeatherApiClient.Models;

namespace NovaVoice.AccuWeatherApiClient;

public interface IAccuWeatherApiClient : IDisposable
{
    Task<WeatherResponse> GetWeatherAsync(string locationKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<LocationResponse>> GetLocationKeyAsync(string query, CancellationToken cancellationToken = default);
}