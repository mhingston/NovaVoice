using System.Net.Http.Json;
using System.Net.Mime;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NovaVoice.AccuWeatherApiClient.Models;
using NovaVoice.Models;

namespace NovaVoice.AccuWeatherApiClient;

public class AccuWeatherApiClient : IAccuWeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly Configuration _configuration;

    public AccuWeatherApiClient(HttpClient httpClient, IOptions<Configuration> configuration)
    {
        _configuration = configuration.Value;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://dataservice.accuweather.com/");
        _httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Accept, MediaTypeNames.Application.Json);
    }
    
    public async Task<WeatherResponse> GetWeatherAsync(string locationKey, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"forecasts/v1/daily/1day/{locationKey}?metric=true&apikey={_configuration.AccuWeather.ApiKey}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var weatherResponse = await response.Content.ReadFromJsonAsync<WeatherResponse>(cancellationToken: cancellationToken);
        return weatherResponse!;
    }

    public async Task<IEnumerable<LocationResponse>> GetLocationKeyAsync(string query, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"locations/v1/search?q={query}&apikey={_configuration.AccuWeather.ApiKey}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var locationResponses = await response.Content.ReadFromJsonAsync<IEnumerable<LocationResponse>>(cancellationToken: cancellationToken);
        return locationResponses!;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}