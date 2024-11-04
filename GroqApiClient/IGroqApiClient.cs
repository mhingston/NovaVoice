using NovaVoice.GroqApiClient.Models;

namespace NovaVoice.GroqApiClient;

public interface IGroqApiClient : IDisposable
{
    Task<CreateChatCompletionResponse> CreateChatCompletionAsync(CreateChatCompletionRequest request, CancellationToken cancellationToken = default);
    Task<CreateTranscriptionResponse> CreateTranscriptionAsync(CreateTranscriptionRequest request, CancellationToken cancellationToken = default);
    Task<CreateTranslationResponse> CreateTranslationAsync(CreateTranslationRequest request, CancellationToken cancellationToken = default);
    Task<ListModelsResponse> ListModelsAsync(CancellationToken cancellationToken = default);
    Task<GetModelResponse> GetModelAsync(string modelId, CancellationToken cancellationToken = default);
}