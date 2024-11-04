namespace NovaVoice.Events;

public class WakeWordDetectedEvent(string wakeWord) : BaseEvent
{
    public readonly string WakeWord = wakeWord;
}