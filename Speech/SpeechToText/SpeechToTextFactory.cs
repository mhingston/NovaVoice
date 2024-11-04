using Microsoft.Extensions.Logging;
using NovaVoice.Models;
using NovaVoice.Speech.Recorder;
using NovaVoice.Speech.SpeechToText.BuiltIn;
using NovaVoice.Speech.SpeechToText.Cheetah;

namespace NovaVoice.Speech.SpeechToText;

public static class SpeechToTextFactory
{
    public static ISpeechToTextProvider Create(
        ILoggerFactory loggerFactory,
        Configuration configuration,
        IRecorder recorder,
        GroqApiClient.GroqApiClient groqClient)
    {
        return configuration.SpeechToTextProvider switch
        {
            SpeechToTextProvider.BuiltIn => new BuiltInProvider(
                loggerFactory.CreateLogger<BuiltInProvider>(),
                configuration,
                recorder,
                groqClient),
            SpeechToTextProvider.Cheetah => new CheetahProvider(loggerFactory.CreateLogger<CheetahProvider>(), configuration),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}