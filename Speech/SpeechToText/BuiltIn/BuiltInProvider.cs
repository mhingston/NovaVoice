using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NovaVoice.Models;
using NovaVoice.Events;
using NovaVoice.GroqApiClient.Models;
using NovaVoice.Speech.Recorder;
using WebRtcVadSharp;

namespace NovaVoice.Speech.SpeechToText.BuiltIn;

public class BuiltInProvider : ISpeechToTextProvider
{
    private readonly ILogger<BuiltInProvider> _logger;
    private readonly Configuration _configuration;
    private readonly IRecorder _recorder;
    private readonly WebRtcVad _voiceActivityDetector;
    private readonly GroqApiClient.GroqApiClient _groqClient;
    private readonly object _lock = new();
    private readonly int _silenceFramesThreshold = 30;
    private readonly int _minimumRecordingSize = 300;
    private readonly int _speechFramesThreshold = 10;
    private int _silenceFrameCount;
    private int _speechFrameCount;
    private bool _speechStarted;
    private bool _isProcessing;

    public event AsyncEventHandler<SpeechDetectedEvent>? OnSpeechDetected;
    
    public BuiltInProvider(
        ILogger<BuiltInProvider> logger, 
        Configuration configuration,
        IRecorder recorder,
        GroqApiClient.GroqApiClient groqClient)
    {
        _logger = logger;
        _configuration = configuration;
        _recorder = recorder;
        
        _voiceActivityDetector = new WebRtcVad
        {
            FrameLength = FrameLength.Is30ms,
            OperatingMode = OperatingMode.VeryAggressive,
            SampleRate = SampleRate.Is16kHz
        };
        _groqClient = groqClient;
        
        _logger.LogInformation("Using BuiltIn for Speech to Text");
    }

    public void Detect(short[] pcmFrame)
    {
        lock (_lock)
        {
            if (_isProcessing)
                return;   
        }

        var hasSpeech = _voiceActivityDetector.HasSpeech(pcmFrame);
        
        if (!hasSpeech)
        {
            lock (_lock)
            {
                _speechFrameCount = 0;
                _silenceFrameCount++;   
            }
            
            if (_speechStarted && _silenceFrameCount >= _silenceFramesThreshold && _recorder.RecordingBuffer.Length > _minimumRecordingSize)
            {
                lock (_lock)
                {
                    _isProcessing = true;
                    StopRecordingAndProcess().Wait();
                    _isProcessing = false;   
                }
            }
        }
        else
        {
            lock (_lock)
            {
                _speechFrameCount++;
                _silenceFrameCount = 0;
            }

            // Only start recording after reaching speech frames threshold
            if (!_speechStarted && _speechFrameCount >= _speechFramesThreshold)
            {
                lock (_lock)
                {
                    _speechStarted = true;
                }
                
                _logger.LogDebug("Voice activity detected");
            }
        }
    }

    private async Task StopRecordingAndProcess()
    {
        lock (_lock)
        {
            _speechStarted = false;
            _speechFrameCount = 0;
            _silenceFrameCount = 0;   
        }
        
        _logger.LogDebug("Voice activity ended");

        try
        {
            var waveFormat = new WaveFormat(Constants.SampleRate, Constants.BitsPerSample, Constants.Channels);
            using var wavStream = new MemoryStream();
            await using var writer = new WaveFileWriter(wavStream, waveFormat);
            writer.WriteSamples(_recorder.RecordingBuffer, 0, _recorder.RecordingBuffer.Length);
            await writer.FlushAsync();
            var wavFile = wavStream.ToArray();
            
            using var ms = new MemoryStream(wavFile);
            var response = await _groqClient.CreateTranscriptionAsync(new CreateTranscriptionRequest
            {
                File = ms,
                FileName = "speech.wav",
                Model = _configuration.Groq.WhisperModel!
            });

            var transcribedText = response.Text.Trim();
            
            if (!string.IsNullOrWhiteSpace(transcribedText))
                OnSpeechDetected?.Invoke(this, new SpeechDetectedEvent(transcribedText));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing speech recording");
        }
    }

    public void Dispose()
    {
        
    }
}