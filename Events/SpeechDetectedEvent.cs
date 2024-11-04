namespace NovaVoice.Events;

public class SpeechDetectedEvent(string transcript) : BaseEvent
{
    public readonly string Transcript = transcript;
}