using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Pv;
using NovaVoice.Events;
using NovaVoice.Models;
using NovaVoice.Speech.Recorder;

namespace NovaVoice.Speech.SpeechToText.Cheetah;

public class CheetahProvider : ISpeechToTextProvider
{
    private readonly ILogger<CheetahProvider> _logger;
    private readonly Configuration _configuration;
    private readonly Pv.Cheetah _cheetah;

    public event AsyncEventHandler<SpeechDetectedEvent>? OnSpeechDetected;
    
    public CheetahProvider(ILogger<CheetahProvider> logger, Configuration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        _cheetah = Pv.Cheetah.Create(_configuration.Picovoice!.AccessKey);
        
        _logger.LogInformation("Using Cheetah for Speech to Text");
    }

    public void Detect(short[] pcmFrame)
    {
        try
        {
            var result = _cheetah.Process(pcmFrame);
            
            if (result.IsEndpoint)
            {
                var finalTranscript = _cheetah.Flush();

                if (finalTranscript != null)
                {
                    OnSpeechDetected?.Invoke(this, new SpeechDetectedEvent(finalTranscript.Transcript.Trim()));   
                }
            }
        }
        
        catch (CheetahActivationLimitException)
        {
            _logger.LogInformation($"Picovoice AccessKey '{_configuration.Picovoice!.AccessKey}' has reached its Cheetah processing limit.");
        }
    }

    public void Dispose()
    {
        _cheetah.Dispose();
    }
}