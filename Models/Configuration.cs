using Pv;
using NovaVoice.Speech;

namespace NovaVoice.Models;

public class Configuration
{
    public PicovoiceOptions? Picovoice { get; set; }

    public AzureSpeechOptions? AzureSpeech { get; set; }
    public RecordingEngine Recorder { get; set; }
    public WakeWordProvider WakeWordProvider { get; set; }
    public SpeechToTextProvider SpeechToTextProvider { get; set; }
    public TextToSpeechProvider TextToSpeechProvider { get; set; }
    public bool SaveRecordings { get; set; }
    public int FrameLength { get; set; } = 512;
    public string? VoiceName { get; set; }
    public GoogleOptions Google { get; set; }
    public AccuWeatherOptions AccuWeather { get; set; }
    public GroqOptions Groq { get; set; }
    public int WakeWordTimeoutSeconds { get; set; } = 5;
    public int MessageHistoryTimeoutSeconds { get; set; } = 300;
}