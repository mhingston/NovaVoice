using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NovaVoice.GroqApiClient.Models;
using NovaVoice.Models;

namespace NovaVoice.GroqApiClient;

public class GroqApiClient : IGroqApiClient
{
    private readonly HttpClient _httpClient;
    private readonly Configuration _configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GroqApiClient(HttpClient httpClient, IOptions<Configuration> configuration)
    {
        _configuration = configuration.Value;
        var assemblyName = GetType().Assembly.GetName();
        var version = assemblyName.Version?.ToString();
        var userAgent = nameof(assemblyName.Name);
        
        if(version is not null)
            userAgent += $"/{version}";
        
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.groq.com/");
        _httpClient.DefaultRequestHeaders.Add(
            HeaderNames.UserAgent, userAgent);
        _httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Accept, MediaTypeNames.Application.Json);
        _httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Authorization, $"Bearer {_configuration.Groq.ApiKey}");
    }
    
    public async Task<CreateChatCompletionResponse> CreateChatCompletionAsync(CreateChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync("openai/v1/chat/completions", JsonContent.Create(request, options: _jsonSerializerOptions), cancellationToken);
        response.EnsureSuccessStatusCode();
        var completion = await response.Content.ReadFromJsonAsync<CreateChatCompletionResponse>(cancellationToken: cancellationToken);
        return completion!;
    }
    
    public async Task<CreateTranslationResponse> CreateTranslationAsync(CreateTranslationRequest request, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(request.File), "file", request.FileName);
        content.Add(new StringContent(request.Model), "model");
        
        if(!string.IsNullOrEmpty(request.Prompt))
            content.Add(new StringContent(request.Prompt), "prompt");
        
        if(!string.IsNullOrEmpty(request.ResponseFormat))
            content.Add(new StringContent(request.ResponseFormat), "response_format");
        
        if(request.Temperature.HasValue)
            content.Add(new StringContent($"{request.Temperature.Value:R}"), "temperature");
        
        var response = await _httpClient.PostAsync("openai/v1/audio/translations", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var transcription = await response.Content.ReadFromJsonAsync<CreateTranslationResponse>(cancellationToken: cancellationToken);
        return transcription!;
    }
    
    public async Task<CreateTranscriptionResponse> CreateTranscriptionAsync(CreateTranscriptionRequest request, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(request.File), "file", request.FileName);
        content.Add(new StringContent(request.Model), "model");
        
        if(!string.IsNullOrEmpty(request.Language))
            content.Add(new StringContent(request.Language), "language");
        
        if(!string.IsNullOrEmpty(request.Prompt))
            content.Add(new StringContent(request.Prompt), "prompt");
        
        if(!string.IsNullOrEmpty(request.ResponseFormat))
            content.Add(new StringContent(request.ResponseFormat), "response_format");
        
        if(request.Temperature.HasValue)
            content.Add(new StringContent($"{request.Temperature.Value:R}"), "temperature");
        
        if(request.TimestampGranularities?.Any() == true)
            content.Add(new StringContent(string.Join(",", request.TimestampGranularities)), "timestamp_granularities");
        
        var response = await _httpClient.PostAsync("openai/v1/audio/transcriptions", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var transcription = await response.Content.ReadFromJsonAsync<CreateTranscriptionResponse>(cancellationToken: cancellationToken);
        return transcription!;
    }

    public async Task<ListModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("openai/v1/models", cancellationToken);
        response.EnsureSuccessStatusCode();
        var models = await response.Content.ReadFromJsonAsync<ListModelsResponse>(cancellationToken: cancellationToken);
        return models!;
    }
    
    public async Task<GetModelResponse> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"openai/v1/models/{modelId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var model = await response.Content.ReadFromJsonAsync<GetModelResponse>(cancellationToken: cancellationToken);
        return model!;
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}