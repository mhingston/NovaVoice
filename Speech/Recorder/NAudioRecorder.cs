using System.Buffers;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using NovaVoice.Models;

namespace NovaVoice.Speech.Recorder;

public class NAudioRecorder : BaseRecorder
{
    private readonly ILogger<NAudioRecorder> _logger;
    private readonly Configuration _configuration;
    private readonly WaveInEvent _waveIn;
    private readonly Queue<byte> _orphanedFrames;
    private readonly int _frameLength;
    private readonly int _frameSizeInBytes;
    private readonly object _recordingLock = new();
    private volatile bool _isRecording;
    private readonly ArrayPool<byte> _bytePool = ArrayPool<byte>.Shared;
    private readonly ArrayPool<short> _shortPool = ArrayPool<short>.Shared;

    public NAudioRecorder(
        ILogger<NAudioRecorder> logger,
        Configuration configuration) : base(logger, configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _frameLength = configuration.FrameLength;
        _frameSizeInBytes = _frameLength * sizeof(short);
        _orphanedFrames = new Queue<byte>(_frameSizeInBytes);

        const int bufferedFramesCount = 50;
        var msPerFrame = (_frameLength * 1000.0) / Constants.SampleRate;
        var bufferMs = (int)Math.Ceiling(msPerFrame * bufferedFramesCount);

        try
        {
            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(Constants.SampleRate, Constants.Channels),
                BufferMilliseconds = bufferMs
            };
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize WaveInEvent");
            throw;
        }

        _logger.LogInformation("Using NAudio for Recording");
    }

    public override void StartRecording(CancellationToken cancellationToken = default)
    {
        lock (_recordingLock)
        {
            if (_isRecording) return;

            try
            {
                _orphanedFrames.Clear();
                _waveIn.StartRecording();
                _isRecording = true;
                _logger.LogDebug("Started recording");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start recording");
                throw;
            }
        }
    }

    public override void StopRecording()
    {
        lock (_recordingLock)
        {
            if (!_isRecording) return;

            try
            {
                _waveIn.StopRecording();
                _isRecording = false;
                _logger.LogDebug("Stopped recording");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop recording");
                throw;
            }
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (!_isRecording) return;

        try
        {
            // Calculate total buffer size needed including orphaned frames
            var totalSize = e.BytesRecorded + _orphanedFrames.Count;
            var data = _bytePool.Rent(totalSize);
            
            try
            {
                var dataSpan = new Span<byte>(data, 0, totalSize);
                var offset = 0;

                // Copy orphaned frames if any
                if (_orphanedFrames.Count > 0)
                {
                    _orphanedFrames.CopyTo(data, 0);
                    offset = _orphanedFrames.Count;
                    _orphanedFrames.Clear();
                }

                // Copy new data
                new ReadOnlySpan<byte>(e.Buffer, 0, e.BytesRecorded).CopyTo(dataSpan[offset..]);

                var totalBytes = totalSize;
                var processedBytes = 0;

                // Process complete frames
                while (processedBytes + _frameSizeInBytes <= totalBytes)
                {
                    var pcmFrame = _shortPool.Rent(_frameLength);
                    try
                    {
                        var frameSpan = new Span<short>(pcmFrame, 0, _frameLength);
                        var sourceSpan = dataSpan.Slice(processedBytes, _frameSizeInBytes);
                        
                        for (var i = 0; i < _frameLength; i++)
                        {
                            frameSpan[i] = BitConverter.ToInt16(sourceSpan.Slice(i * sizeof(short), sizeof(short)));
                        }

                        processedBytes += _frameSizeInBytes;
                        
                        // Create a copy of the frame data for the event
                        var frameCopy = new short[_frameLength];
                        frameSpan.CopyTo(frameCopy);
                        RaisePcmFrameReceived(frameCopy);
                    }
                    finally
                    {
                        _shortPool.Return(pcmFrame);
                    }
                }

                // Store remaining bytes as orphaned frames
                if (processedBytes < totalBytes)
                {
                    var remainingSpan = dataSpan.Slice(processedBytes);
                    foreach (var b in remainingSpan)
                    {
                        _orphanedFrames.Enqueue(b);
                    }
                }
            }
            finally
            {
                _bytePool.Return(data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audio data");
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            _logger.LogError(e.Exception, "Recording stopped due to error");
        }

        _isRecording = false;
    }

    public override void Dispose()
    {
        try
        {
            StopRecording();
            _waveIn.DataAvailable -= OnDataAvailable;
            _waveIn.RecordingStopped -= OnRecordingStopped;
            _waveIn.Dispose();
            _orphanedFrames.Clear();
        }
        
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during disposal");
        }

        base.Dispose();
    }
}
