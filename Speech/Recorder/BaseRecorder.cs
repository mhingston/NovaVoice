using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NovaVoice.Events;
using NovaVoice.Models;

namespace NovaVoice.Speech.Recorder;

public abstract class BaseRecorder : IRecorder
{
    private readonly ILogger _logger;
    private readonly Configuration _configuration;
    private bool _isBuffering;
    private readonly List<IDisposable> _subscriptions = [];
    private readonly List<short[]> _buffer = [];
    private readonly object _bufferLock = new();
    
    public short[] RecordingBuffer
    {
        get
        {
            lock (_bufferLock)
            {
                return _buffer.SelectMany(x => x).ToArray();
            }
        }
    }

    public event EventHandler<AudioInputEvent>? OnPcmFrameReceived;

    protected BaseRecorder(
        ILogger logger, 
        Configuration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected void RaisePcmFrameReceived(short[] pcmFrame)
    {
        OnPcmFrameReceived?.Invoke(this, new AudioInputEvent(pcmFrame));

        if (_isBuffering)
        {
            lock (_bufferLock)
            {
                _buffer.Add(pcmFrame);
            }
        }
    }

    public void OnWakeWordDetected(WakeWordDetectedEvent evt)
    {
        _buffer.Clear();
        _isBuffering = true;
        _logger.LogDebug("Started buffering audio");
    }

    public void OnSpeechDetected(SpeechDetectedEvent evt)
    {
        _isBuffering = false;
        SaveBufferToFile();
        _logger.LogDebug("Stopped buffering audio");
    }
    
    public void CancelRecording()
    {
        lock (_bufferLock)
        {
            _buffer.Clear();
            _isBuffering = false;
            _logger.LogDebug("Cancelled buffering audio");
        }
    }

    private void SaveBufferToFile()
    {
        try
        {
            short[] bufferCopy;
            lock (_bufferLock)
            {
                bufferCopy = _buffer.SelectMany(x => x).ToArray();
            }

            var waveFormat = new WaveFormat(Constants.SampleRate, Constants.BitsPerSample, Constants.Channels);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var outputDirectory = Path.Join(Directory.GetCurrentDirectory(), "Recordings");
            Directory.CreateDirectory(outputDirectory);
            var outputPath = Path.Join(outputDirectory, $"recording_{timestamp}.wav");

            using var writer = new WaveFileWriter(outputPath, waveFormat);
            writer.WriteSamples(bufferCopy, 0, bufferCopy.Length);

            _logger.LogInformation("Saved recording to {OutputPath}", outputPath);
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save recording");
        }
        
        finally
        {
            lock (_bufferLock)
            {
                _buffer.Clear();
            }
        }
    }

    public abstract void StartRecording(CancellationToken cancellationToken = default);
    public abstract void StopRecording();

    public virtual void Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }
    }
}