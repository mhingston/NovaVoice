using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Pv;
using NovaVoice.Events;
using NovaVoice.Models;

namespace NovaVoice.Speech.WakeWord.Porcupine;

public class PorcupineProvider : IWakeWordProvider
{
    private readonly ILogger<PorcupineProvider> _logger;
    private readonly Configuration _configuration;
    private readonly List<string> _keywordPaths = [];
    private readonly Pv.Porcupine _porcupine;
    
    public event AsyncEventHandler<WakeWordDetectedEvent>? OnWakeWordDetected;
    
    public PorcupineProvider(
        ILogger<PorcupineProvider> logger,
        Configuration configuration)
    {
        _logger = logger;
        _configuration = configuration;

        string platform;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            platform = "windows";
        
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            platform = "linux";

        else
            throw new PlatformNotSupportedException("Porcupine is not supported on this platform");
        
        foreach (var keyword in _configuration.Picovoice?.Porcupine!.Keywords!)
        {
            _keywordPaths.Add(Enum.TryParse<BuiltInKeyword>(keyword, true, out var builtInKeyword)
                ? Pv.Porcupine.BUILT_IN_KEYWORD_PATHS[builtInKeyword]
                : Path.Join(Directory.GetCurrentDirectory(), "Speech", "WakeWord", "Porcupine", "Models", platform, $"{keyword}.ppn"));
        }
        
        _porcupine = Pv.Porcupine.FromKeywordPaths(_configuration.Picovoice!.AccessKey, _keywordPaths);
        
        _logger.LogInformation("Using Porcupine for Wake Word Detection");
        _logger.LogInformation($"Listening for wake words: {string.Join(", ", _configuration.Picovoice.Porcupine!.Keywords)}");
    }

    public void Detect(short[] pcmFrame)
    {
        var keywordIndex = _porcupine.Process(pcmFrame);

        if (keywordIndex < 0) return;
        
        var wakeWord = _configuration.Picovoice!.Porcupine!.Keywords[keywordIndex];
        OnWakeWordDetected?.Invoke(this, new WakeWordDetectedEvent(wakeWord));
    }

    public void Dispose()
    {
        
    }
}