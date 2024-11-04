using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NovaVoice.Events;
using NovaVoice.GroqApiClient.Models;
using NovaVoice.Models;
using NovaVoice.Speech.Recorder;
using NovaVoice.Speech.SpeechToText;
using NovaVoice.Speech.TextToSpeech;
using NovaVoice.Speech.WakeWord;
using NovaVoice.Tools;

namespace NovaVoice;

public class Assistant : BackgroundService
{
    private readonly ILogger<Assistant> _logger;
    private readonly Configuration _configuration;
    private readonly IRecorder _recorder;
    private readonly IWakeWordProvider _wakeWordProvider;
    private readonly ISpeechToTextProvider _speechToTextProvider;
    private readonly ITextToSpeechProvider _textToSpeechProvider;
    private readonly GroqApiClient.GroqApiClient _groqClient;
    private readonly IEnumerable<ITool> _tools;
    private readonly YoutubePlayer _youtubePlayer;
    private readonly IServiceProvider _serviceProvider;
    private readonly System.Timers.Timer _wakeWordTimeoutTimer;
    private readonly ConcurrentQueue<MessageContainer> _messageHistory = new();
    private readonly SemaphoreSlim _processingLock = new(1, 1);
    private readonly Channel<short[]> _pcmFrameChannel = Channel.CreateBounded<short[]>(new BoundedChannelOptions(150)
    {
        FullMode = BoundedChannelFullMode.Wait
    });
    private bool _isReady;
    private bool _wakeWordDetected;
    private CancellationTokenSource _cts = new();

    public Assistant(
        ILogger<Assistant> logger,
        ILoggerFactory loggerFactory,
        IOptions<Configuration> configuration,
        GroqApiClient.GroqApiClient groqClient,
        IEnumerable<ITool> tools,
        YoutubePlayer youtubePlayer,
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        _logger = logger;
        _configuration = configuration.Value;
        _groqClient = groqClient;
        _tools = tools;
        _youtubePlayer = youtubePlayer;
        _serviceProvider = serviceProvider;
        hostApplicationLifetime.ApplicationStopped.Register(OnStopped);

        _recorder = RecorderFactory.Create(loggerFactory, _configuration);
        _wakeWordProvider = WakeWordFactory.Create(loggerFactory, _configuration, _recorder);
        _speechToTextProvider = SpeechToTextFactory.Create(loggerFactory, _configuration, _recorder, _groqClient);
        _textToSpeechProvider = TextToSpeechFactory.Create(loggerFactory, _configuration);

        _recorder.OnPcmFrameReceived += OnPcmFrameReceived;
        _wakeWordProvider.OnWakeWordDetected += OnWakeWordDetected;
        _speechToTextProvider.OnSpeechDetected += OnSpeechDetected;

        _wakeWordTimeoutTimer = new System.Timers.Timer(TimeSpan.FromSeconds(_configuration.WakeWordTimeoutSeconds));
        _wakeWordTimeoutTimer.Elapsed += WakeWordTimeoutTimer_Elapsed;
        _wakeWordTimeoutTimer.AutoReset = false;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Start the processing loop
            _ = Task.Run(() => ProcessPcmFramesAsync(_cts.Token), cancellationToken);

            _recorder.StartRecording(cancellationToken);

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in Assistant");
            throw;
        }
    }

    private void OnPcmFrameReceived(object? sender, AudioInputEvent e)
    {
        if (_cts.Token.IsCancellationRequested)
            return;

        if (!_pcmFrameChannel.Writer.TryWrite(e.PcmFrame))
        {
            _logger.LogWarning("Failed to write PCM frame to channel.");
        }
    }

    private async Task ProcessPcmFramesAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!_isReady)
            {
                _logger.LogInformation("Assistant ready");
                await SoundPlayer.PlayAsync(Constants.ReadySound);
                _isReady = true;
            }

            await foreach (var frame in _pcmFrameChannel.Reader.ReadAllAsync(cancellationToken))
            {
                // Process the frame
                _wakeWordProvider.Detect(frame);

                if (_wakeWordDetected)
                {
                    _speechToTextProvider.Detect(frame);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Graceful shutdown
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PCM frames");
            Dispose();
        }
    }

    private async Task OnWakeWordDetected(object? sender, WakeWordDetectedEvent e)
    {
        try
        {
            if (_cts.Token.IsCancellationRequested)
                return;

            _logger.LogInformation("Wake Word Detected: {WakeWord}", e.WakeWord);
            await _textToSpeechProvider.StopAsync();
            await _youtubePlayer.StopAsync();
            await SoundPlayer.PlayAsync(Constants.WakeWordDetectedSound);

            if (_configuration.SaveRecordings)
                _recorder.OnWakeWordDetected(e);

            _wakeWordDetected = true;

            _wakeWordTimeoutTimer.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing wake word");
            await SoundPlayer.PlayAsync(Constants.ErrorSound);
        }
    }

    private void WakeWordTimeoutTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _logger.LogInformation("Wake word timeout elapsed.");
        _wakeWordDetected = false;
        _recorder.CancelRecording();
        Task.Run(() => SoundPlayer.PlayAsync(Constants.ErrorSound));
    }

    private async Task OnSpeechDetected(object? sender, SpeechDetectedEvent e)
    {
        try
        {
            if (_cts.Token.IsCancellationRequested)
                return;

            _logger.LogInformation("Detected Speech: {Transcript}", e.Transcript);
            _wakeWordTimeoutTimer.Stop();

            if (_configuration.SaveRecordings)
                _recorder.OnSpeechDetected(e);

            await SoundPlayer.PlayAsync(Constants.SpeechDetectedSound);
            _wakeWordDetected = false;

            await ProcessRequestAsync(e.Transcript);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing speech");
            await SoundPlayer.PlayAsync(Constants.ErrorSound);
        }
    }

    private void OnStopped()
    {
        _logger.LogInformation("Stopped");
    }

    private async Task ProcessRequestAsync(string userPrompt)
    {
        try
        {
            await _processingLock.WaitAsync();
            PruneExpiredMessages();

            var messages = new List<Message>
            {
                new()
                {
                    Role = "system",
                    Content = _configuration.Groq.SystemPrompt
                }
            };
            
            var previousMessages = _messageHistory.Select(x => x.Message).ToList();

            if (previousMessages.Any())
                messages.AddRange(previousMessages);
            
            var userMessage = new Message
            {
                Role = "user",
                Content = userPrompt
            };
            messages.Add(userMessage);
            _messageHistory.Enqueue(new MessageContainer(userMessage));
            
            var request = new CreateChatCompletionRequest
            {
                Model = _configuration.Groq.ToolUseModel!,
                Messages = messages,
                Tools = _tools
            };

            var firstResponse = await _groqClient.CreateChatCompletionAsync(request, _cts.Token);
            _messageHistory.Enqueue(new MessageContainer(firstResponse.Choices.First().Message));

            if (firstResponse.Choices.First().FinishReason == "tool_calls")
            {
                foreach (var toolCall in firstResponse.Choices.First().Message.ToolCalls!)
                {
                    var tool = _tools.First(t => t.Function.Name == toolCall.Function!.Name);
                    var args = JsonSerializer.Deserialize<Dictionary<string, object>>(toolCall.Function!.Arguments!);
                    args!.Add("id", toolCall.Id!);
                    var toolResult = await tool.Function.ExecuteAsync(args, _cts.Token);
                    _messageHistory.Enqueue(new MessageContainer(toolResult));

                    // If the tool has a post-execute function, call it and don't both generating another completion
                    if (tool.PostExecuteAsync != null)
                    {
                        await tool.PostExecuteAsync(toolResult.Content!, _serviceProvider, _textToSpeechProvider);
                        return;
                    }

                    request.Messages.Add(toolResult);
                }

                var secondResponse = await _groqClient.CreateChatCompletionAsync(request, _cts.Token);
                _messageHistory.Enqueue(new MessageContainer(secondResponse.Choices.First().Message));
                await _textToSpeechProvider.SpeakAsync(secondResponse.Choices.First().Message.Content!);
            }
            else
            {
                await _textToSpeechProvider.SpeakAsync(firstResponse.Choices.First().Message.Content!);
            }
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
            await SoundPlayer.PlayAsync(Constants.ErrorSound);
        }
        
        finally 
        {
            _processingLock.Release();
        }
    }
    
    private void PruneExpiredMessages()
    {
        while (_messageHistory.TryPeek(out var message) &&
               DateTime.UtcNow - message.Timestamp > TimeSpan.FromMinutes(_configuration.MessageHistoryTimeoutSeconds))
        {
            _messageHistory.TryDequeue(out _);
        }
    }

    public override void Dispose()
    {
        try
        {
            _logger.LogInformation("Assistant shutdown requested");
            _cts.Cancel();
            
            _pcmFrameChannel.Writer.Complete();

            SoundPlayer.PlayAsync(Constants.ExitingSound, _logger).Wait();
            _wakeWordTimeoutTimer.Dispose();
            _cts.Dispose();
            _recorder.Dispose();
            _speechToTextProvider.Dispose();
            _groqClient.Dispose();
        }
        
        finally
        {
            base.Dispose();
        }
    }
}
