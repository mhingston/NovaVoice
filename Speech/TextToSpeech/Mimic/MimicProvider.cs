using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using NovaVoice.Models;

namespace NovaVoice.Speech.TextToSpeech.Mimic;

public class MimicProvider : ITextToSpeechProvider
{
    private readonly ILogger<MimicProvider> _logger;
    private readonly Configuration _configuration;
    private Process? _process;
    private readonly string? _voice;
    
    private const decimal SpeechRate = 0.8m;
    
    public MimicProvider(ILogger<MimicProvider> logger, Configuration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        _logger.LogInformation("Using Mimic for Text To Speech.");

        if (configuration.VoiceName != null)
        {
            _logger.LogInformation($"Target Voice: {configuration.VoiceName}");
            _voice = GetVoiceAsync(configuration.VoiceName).Result;
            _logger.LogInformation($"Selected Voice: {_voice}");   
        }
    }
    
    public async Task SpeakAsync(string text)
    {
        Dispose();
        _process = new Process();
        var voice = _voice != null ? $"--voice {_voice}" : "";
        var command = $"mimic3 {voice} --length-scale {SpeechRate} --interactive \"{text.Replace("\"", "")}\"";
        _process.StartInfo = new ProcessStartInfo("bash", $"-c '{command}'")
        {
            RedirectStandardOutput = false,
            UseShellExecute = true,
            CreateNoWindow = false
        };
        _logger.LogInformation($"Running: {_process.StartInfo.FileName} {_process.StartInfo.Arguments}");
        _process.Start();
        await _process.WaitForExitAsync();
    }

    public Task StopAsync()
    {
        _logger.LogInformation("Speech Cancelled");   
        _process?.Kill(true);
        return Task.CompletedTask;
    }

    private async Task<string> GetVoiceAsync(string? voiceName)
    {
        var voices = await GetVoicesAsync();
        
        if (voiceName != null)
        {
            var matchingVoice = voices.FirstOrDefault(v => v.Name == voiceName);

            if (matchingVoice != null)
                return matchingVoice.ToString();
        }

        var fallBackVoice = voices
            .OrderBy(v => v.Culture.TwoLetterISOLanguageName == "en" ? 0 : 1)
            .ThenBy(v => v.Culture.Name == "en-GB" ? 0 : 1)
            .First();

        return fallBackVoice.ToString();
    }

    private async Task<IEnumerable<MimicVoice>> GetVoicesAsync()
    {
        var voices = new List<MimicVoice>();
        var process = new Process();
        process.StartInfo = new ProcessStartInfo("mimic3", "--voices")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        foreach (var line in output.Split('\n').Skip(1))
        {
            var parts = line.Split('\t');
            
            if (parts.Length >= 3)
            {
                var name = parts[2];
                var culture = ParseCulture(parts[1]);
                voices.Add(new MimicVoice
                {
                    Name = name,
                    Culture = culture,
                });
            }
        }

        return voices;
    }

    private CultureInfo ParseCulture(string language)
    {
        var cultureCode = language.Replace("-", "_");
        return new CultureInfo(cultureCode);
    }

    public void Dispose()
    {
        _process?.Kill(true);
        _process?.Dispose();
    }
}