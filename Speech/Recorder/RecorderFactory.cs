using Microsoft.Extensions.Logging;
using NovaVoice.Events;
using NovaVoice.Models;

namespace NovaVoice.Speech.Recorder;

public static class RecorderFactory
{
    public static IRecorder Create(ILoggerFactory loggerFactory, Configuration configuration)
    {
        return configuration.Recorder switch
        {
            RecordingEngine.NAudio => new NAudioRecorder(
                loggerFactory.CreateLogger<NAudioRecorder>(),
                configuration),
            RecordingEngine.Picovoice => new PicovoiceRecorder(
                loggerFactory.CreateLogger<PicovoiceRecorder>(),
                configuration),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}