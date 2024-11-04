namespace NovaVoice.Events;

public abstract class BaseEvent : EventArgs
{
    public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;
}