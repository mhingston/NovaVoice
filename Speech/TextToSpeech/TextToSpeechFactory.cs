using Microsoft.Extensions.Logging;
using NovaVoice.Models;
using NovaVoice.Speech.TextToSpeech.Mimic;

namespace NovaVoice.Speech.TextToSpeech;

public static class TextToSpeechFactory
{
    public static ITextToSpeechProvider Create(
        ILoggerFactory loggerFactory,
        Configuration configuration)
    {
        return configuration.TextToSpeechProvider switch
        {
            TextToSpeechProvider.WindowsSpeech => new WindowsSpeech.WindowsSpeechProvider(loggerFactory.CreateLogger<WindowsSpeech.WindowsSpeechProvider>(), configuration),
            TextToSpeechProvider.Mimic => new MimicProvider(loggerFactory.CreateLogger<MimicProvider>(), configuration),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}