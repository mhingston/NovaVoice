using System.Globalization;

namespace NovaVoice.Speech.TextToSpeech.Mimic;

public class MimicVoice
{
    public string Name { get; set; }
    public CultureInfo Culture { get; set; }
    
    public override string ToString() => $"{Culture.Name.Replace("-", "_")}/{Name}";
};