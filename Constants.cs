namespace NovaVoice;

public static class Constants
{
    // Notification sounds
    public const string ReadySound = "assistantReady.mp3";
    public const string WakeWordDetectedSound = "wakeWordDetected.mp3";
    public const string SpeechDetectedSound = "speechDetected.mp3";
    public const string ErrorSound = "error.mp3";
    public const string ExitingSound = "assistantExiting.mp3";

    // Audio format constants
    public const ushort SampleRate = 16000; // 16KHz sample rate
    public const ushort Channels = 1;
    public const ushort BitsPerSample = 16; // Bits per sample (e.g., 16-bit audio)
}