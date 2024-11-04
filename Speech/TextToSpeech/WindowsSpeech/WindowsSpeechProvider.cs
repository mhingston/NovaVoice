using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using Microsoft.Extensions.Logging;
using NovaVoice.Models;

namespace NovaVoice.Speech.TextToSpeech.WindowsSpeech;

public class WindowsSpeechProvider : ITextToSpeechProvider
{
    private readonly ILogger<WindowsSpeechProvider> _logger;
    private readonly Configuration _configuration;
    private SpeechSynthesizer? _synthesizer;
    private readonly string? _voice;
    
    public WindowsSpeechProvider(ILogger<WindowsSpeechProvider> logger, Configuration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            throw new PlatformNotSupportedException("Windows Speech is only supported on Windows.");
        
        _logger.LogInformation("Using Windows Speech for Text To Speech.");

        if (configuration.VoiceName != null)
        {
            _logger.LogInformation($"Target Voice: {configuration.VoiceName}");
            _voice = GetVoice(configuration.VoiceName);
            _logger.LogInformation($"Selected Voice: {_voice}");
        }

        Initialise();
    }

    private void Initialise()
    {
        _synthesizer = new SpeechSynthesizer();
        _synthesizer.SetOutputToDefaultAudioDevice();
        
        if(_voice != null)
            _synthesizer.SelectVoice(_voice);
    }
    
    public Task SpeakAsync(string text)
    {
        if (_synthesizer == null)
            Initialise();

        try
        {
            _synthesizer.Speak(text);
        }
        
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Speech Cancelled");    
        }
        
        return Task.CompletedTask;
    }

    public Task StopAsync() => Task.Run(Dispose);

    private string GetVoice(string? voiceName)
    {
        var voices = _synthesizer.GetInstalledVoices();

        if (voiceName != null)
        {
            var matchingVoice = voices.FirstOrDefault(v => v.VoiceInfo.Name == voiceName);
        
            if(matchingVoice != null)
                return matchingVoice.VoiceInfo.Name;   
        }

        var fallBackVoice = voices
            .OrderBy(v => v.VoiceInfo.Culture.TwoLetterISOLanguageName == "en" ? 0 : 1)
            .ThenBy(v => v.VoiceInfo.Gender == VoiceGender.Male ? 0 : 1)
            .ThenBy(v => v.VoiceInfo.Culture.Name == "en-GB" ? 0 : 1)
            .First();

        return fallBackVoice.VoiceInfo.Name;
    }

    public void Dispose()
    {
        _synthesizer?.Dispose();
        _synthesizer = null;
    }
}