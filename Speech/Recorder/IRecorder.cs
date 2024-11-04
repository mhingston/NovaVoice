using NovaVoice.Events;

namespace NovaVoice.Speech.Recorder;

public interface IRecorder : IDisposable
{
    event EventHandler<AudioInputEvent>? OnPcmFrameReceived; 
    short[] RecordingBuffer { get; }
    void StartRecording(CancellationToken cancellationToken = default);
    void OnWakeWordDetected(WakeWordDetectedEvent evt);
    void OnSpeechDetected(SpeechDetectedEvent evt);
    void CancelRecording();
    void StopRecording();
};