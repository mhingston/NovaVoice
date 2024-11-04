using NovaVoice.Events;

namespace NovaVoice.Speech.WakeWord;

public interface IWakeWordProvider : IDisposable
{
    event AsyncEventHandler<WakeWordDetectedEvent>? OnWakeWordDetected;
    void Detect(short[] pcmFrame);
}