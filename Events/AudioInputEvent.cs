namespace NovaVoice.Events;

public class AudioInputEvent(short[] pcmFrame) : BaseEvent
{
    public readonly short[] PcmFrame = pcmFrame;
}