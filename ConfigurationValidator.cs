using FluentValidation;
using NovaVoice.Models;

namespace NovaVoice;

public class ConfigurationValidator : AbstractValidator<Configuration>
{
    public ConfigurationValidator()
    {
        RuleFor(x => x.Google.ApiKey)
            .NotEmpty()
            .WithMessage("`Google.ApiKey` is not configured");

        RuleFor(x => x.Google.CustomSearchId)
            .NotEmpty()
            .WithMessage("`Google.CustomSearchId` is not configured");

        RuleFor(x => x.AccuWeather.ApiKey)
            .NotEmpty()
            .WithMessage("`AccuWeather.ApiKey` is not configured");

        RuleFor(x => x.Groq.ApiKey)
            .NotEmpty()
            .WithMessage("`Groq.ApiKey` is not configured");

        RuleFor(x => x.Groq.GeneralModel)
            .NotEmpty()
            .WithMessage("`Groq.GeneralModel` is not configured");

        RuleFor(x => x.Groq.ToolUseModel)
            .NotEmpty()
            .WithMessage("`Groq.ToolUseModel` is not configured");

        RuleFor(x => x.Groq.SystemPrompt)
            .NotEmpty()
            .WithMessage("`Groq.SystemPrompt` is not configured");

        When(x => x.SpeechToTextProvider == SpeechToTextProvider.BuiltIn, () =>
        {
            RuleFor(x => x.Groq.WhisperModel)
                .NotEmpty()
                .WithMessage("`Groq.WhisperModel` is not configured");
        });

        When(x => x.WakeWordProvider == WakeWordProvider.Porcupine || 
                  x.SpeechToTextProvider == SpeechToTextProvider.Cheetah, () =>
        {
            RuleFor(x => x.Picovoice.AccessKey)
                .NotEmpty()
                .WithMessage("`Picovoice.AccessKey` is not configured");
        });

        When(x => x.WakeWordProvider == WakeWordProvider.AzureSpeech, () =>
        {
            RuleFor(x => x.AzureSpeech.Keywords)
                .NotEmpty()
                .WithMessage("`AzureSpeech.Keywords` is not configured");
        });

        When(x => x.WakeWordProvider == WakeWordProvider.Porcupine, () =>
        {
            RuleFor(x => x.Picovoice.Porcupine.Keywords)
                .NotEmpty()
                .WithMessage("`Picovoice.Porcupine.Keywords` is not configured");
        });
    }
}