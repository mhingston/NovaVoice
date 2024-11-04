using NovaVoice.Events;

namespace NovaVoice.Speech.SpeechToText;

public interface ISpeechToTextProvider : IDisposable
{
    event AsyncEventHandler<SpeechDetectedEvent>? OnSpeechDetected;
    void Detect(short[] pcmFrame);
}