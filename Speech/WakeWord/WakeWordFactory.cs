using Microsoft.Extensions.Logging;
using NovaVoice.Models;
using NovaVoice.Speech.Recorder;
using NovaVoice.Speech.WakeWord.AzureSpeech;
using NovaVoice.Speech.WakeWord.Porcupine;

namespace NovaVoice.Speech.WakeWord;

public static class WakeWordFactory
{
    public static IWakeWordProvider Create(ILoggerFactory loggerFactory, Configuration configuration, IRecorder recorder)
    {
        return configuration.WakeWordProvider switch
        {
            WakeWordProvider.AzureSpeech => new AzureSpeechProvider(loggerFactory.CreateLogger<AzureSpeechProvider>(),
                configuration),
            WakeWordProvider.Porcupine => new PorcupineProvider(loggerFactory.CreateLogger<PorcupineProvider>(),
                configuration),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}