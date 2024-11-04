using NovaVoice.GroqApiClient.Models;

namespace NovaVoice.Models;

public class MessageContainer
{
    public DateTime Timestamp { get; }
    public Message Message { get; }

    public MessageContainer(Message message)
    {
        Message = message;
        Timestamp = DateTime.UtcNow;
    }
}