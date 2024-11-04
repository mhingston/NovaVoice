namespace NovaVoice.Models;

public class GroqOptions
{
    public string? ApiKey { get; set; }
    public string? WhisperModel { get; set; }
    public string? ToolUseModel { get; set; }
    public string? GeneralModel { get; set; }
    public string? SystemPrompt { get; set; }
}