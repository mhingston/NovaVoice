# NovaVoice

NovaVoice is an AI voice assistant. Current very early in development, contributions welcome!

## Features

- 🎤 Wake Word Detection ([Porcupine](https://github.com/Picovoice/porcupine) or [Azure Speech](https://learn.microsoft.com/en-us/azure/ai-services/speech-service/custom-keyword-basics))
- 🗣️ Speech-to-Text (Built-in using [WebRtcVad](https://github.com/ladenedge/WebRtcVadSharp) and [Whisper Turbo](https://huggingface.co/openai/whisper-large-v3-turbo) or [Cheetah](https://github.com/Picovoice/cheetah))
- 🔊 Text-to-Speech (Windows Speech or [Mimic3](https://mycroft-ai.gitbook.io/docs/mycroft-technologies/mimic-tts/mimic-3))
- 🤖 AI Interactions (via [Groq API](https://groq.com/))
- 🔍 Extensible Tool System (Google Search API, YouTube Data API, AccuWeather API)
- 📝 Conversation History
- 🎵 Media Playback Support
- 🔄 Configurable Audio Processing Pipeline

## Prerequisites

- .NET 8.0 SDK
- Required external dependencies:
    - [yt-dlp](https://github.com/yt-dlp/yt-dlp) (for YouTube playback)
    - [mpv](https://mpv.io/) (for audio playback)
    - [mimic3](https://mycroft-ai.gitbook.io/docs/mycroft-technologies/mimic-tts/mimic-3) (if using Mimic TTS)

## Configuration

Configure the assistant through environment variables or `appsettings.json`:

```json
{
  "Recorder": "Picovoice",
  "WakeWordProvider": "Porcupine",
  "SpeechToTextProvider": "Cheetah",
  "TextToSpeechProvider": "WindowsSpeech",
  "Google": {
    "ApiKey": "YOUR_KEY",
    "CustomSearchId": "YOUR_ID"
  },
  "AccuWeather": {
    "ApiKey": "YOUR_KEY"
  },
  "Groq": {
    "ApiKey": "YOUR_KEY",
    "WhisperModel": "whisper-large-v3-turbo",
    "GeneralModel": "llama-3.1-70b-versatile",
    "ToolUseModel": "llama3-groq-70b-8192-tool-use-preview",
    "SystemPrompt": "You are a helpful, child-friendly voice assistant called Nova. Respond concisely to user prompts (received via Automatic Speech Recognition). You have access to the following tools: `google_search`, `youtube_search`, and `check_weather`. Consider carefully when you should use them.\n\n- Use `youtube_search` to find music and audio content that the user may want to listen to.\n- Use `check_weather` to get the current weather conditions or forecasts for a location.\n- Use `google_search` for retrieving live data, for example news or exchange rates\n\nPrioritise safety and well-being, never provide responses that could be harmful, inappropriate, biased, or misleading. If the prompt is ambiguous, unsafe, or you are unable to provide a helpful response using the available tools, respond with \"Sorry, I can't help with that\"."
  },
  "Picovoice": {
    "AccessKey": "YOUR_KEY",
    "Porcupine": {
      "Keywords": ["Hey Nova", "OK Stop"]
    }
  }
}
```

## Project Structure

- `Speech/` - Core speech processing components
    - `Recorder/` - Audio input handling
    - `SpeechToText/` - Speech recognition implementations
    - `TextToSpeech/` - Speech synthesis implementations
    - `WakeWord/` - Wake word detection providers
- `Tools/` - Extensible tool system implementations
- `Models/` - Data models and configuration classes
- `Events/` - Event handling system

## Getting Started

1. Clone the repository
2. Configure your API keys in `appsettings.json` or environment variables
3. Install required external dependencies
4. Build and run the project:

```bash
dotnet build
dotnet run
```

## Provider Options

### Wake Word Detection
- Porcupine
- Azure Speech

### Speech-to-Text
- Built-in (crude implementation using WebRtcVadSharp and Whisper Turbo)
- Cheetah

### Text-to-Speech
- Windows Speech
- Mimic3

### Recording Engine
- NAudio
- Picovoice

## Tool System

The assistant includes several built-in tools:
- `google_search` - Web search functionality
- `youtube_search` - YouTube content search and playback
- `check_weather` - Weather information retrieval

## Acknowledgments

- Thanks to [UNIVERSFIELD](https://pixabay.com/users/universfield-28281460/) for the sound effects.