namespace NovaVoice.Speech.TextToSpeech;

public interface ITextToSpeechProvider : IDisposable
{
    Task SpeakAsync(string text);
    Task StopAsync();
}