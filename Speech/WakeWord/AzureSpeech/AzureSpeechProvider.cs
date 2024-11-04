using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using NovaVoice.Events;
using NovaVoice.Models;

namespace NovaVoice.Speech.WakeWord.AzureSpeech;

public class AzureSpeechProvider : IWakeWordProvider
{
    private readonly Configuration _configuration;
    private readonly ILogger<AzureSpeechProvider> _logger;
    private readonly List<KeywordRecognitionModel> _keywordRecognitionModels = [];
    private readonly List<KeywordRecognizer> _keywordRecognizers = [];
    private readonly List<Task<KeywordRecognitionResult>> _keywordRecognitionTasks = [];
    private readonly List<PushAudioInputStream> _pushStreams = [];
    private readonly SemaphoreSlim _detectionSemaphore = new(1, 1);
    private bool _isProcessingDetection;
    private bool _isDisposed;
    
    public event AsyncEventHandler<WakeWordDetectedEvent>? OnWakeWordDetected;
    
    public AzureSpeechProvider(ILogger<AzureSpeechProvider> logger, Configuration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        InitializeRecognizers();
        
        _logger.LogInformation("Using AzureSpeech for Wake Word Detection");
        _logger.LogInformation($"Listening for wake words: {string.Join(", ", _configuration.AzureSpeech!.Keywords!)}");
    }

    private void InitializeRecognizers()
    {
        foreach (var keyword in _configuration.AzureSpeech?.Keywords ?? [])
        {
            var model = KeywordRecognitionModel.FromFile(
                Path.Join(Directory.GetCurrentDirectory(), "Speech", "WakeWord", "AzureSpeech", "Models", $"{keyword}.table"));
            _keywordRecognitionModels.Add(model);

            var pushStream = AudioInputStream.CreatePushStream(AudioStreamFormat.GetWaveFormatPCM(16000, 16, 1));
            _pushStreams.Add(pushStream);

            var audioConfig = AudioConfig.FromStreamInput(pushStream);
            var recognizer = new KeywordRecognizer(audioConfig);
            _keywordRecognizers.Add(recognizer);
        }

        InitialiseListeningTasks();
    }

    public void Detect(short[] pcmFrame)
    {
        if (_isDisposed) return;

        // Push audio data to all streams
        var bytes = new byte[pcmFrame.Length * 2];
        Buffer.BlockCopy(pcmFrame, 0, bytes, 0, bytes.Length);
        
        foreach (var pushStream in _pushStreams)
        {
            pushStream.Write(bytes);
        }

        if (!_isProcessingDetection)
        {
            Task.Run(DetectAsync);
        }
    }

    private async Task DetectAsync()
    {
        try
        {
            await _detectionSemaphore.WaitAsync();
            _isProcessingDetection = true;

            var completedTask = await Task.WhenAny(_keywordRecognitionTasks);
            
            if (completedTask is { IsCompletedSuccessfully: true, Result.Reason: ResultReason.RecognizedKeyword })
            {
                var stoppedTasks = _keywordRecognizers.Select(recognizer => recognizer.StopRecognitionAsync());
                await Task.WhenAll(stoppedTasks);
                InitialiseListeningTasks();
                OnWakeWordDetected?.Invoke(this, new WakeWordDetectedEvent(completedTask.Result.Text));
            }
        }
        finally
        {
            _isProcessingDetection = false;
            _detectionSemaphore.Release();
        }
    }

    private void InitialiseListeningTasks()
    {
        _keywordRecognitionTasks.Clear();
        
        for (var i = 0; i < _keywordRecognitionModels.Count; i++) 
        {
            var keywordRecognizer = _keywordRecognizers[i];
            _keywordRecognitionTasks.Add(keywordRecognizer.RecognizeOnceAsync(_keywordRecognitionModels[i]));
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _detectionSemaphore.Dispose();
        
        foreach(var recognizer in _keywordRecognizers)
        {
            recognizer.Dispose();
        }

        foreach(var pushStream in _pushStreams)
        {
            pushStream.Close();
        }
    }
}