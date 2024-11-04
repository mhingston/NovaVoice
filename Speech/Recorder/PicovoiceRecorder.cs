using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pv;
using NovaVoice.Events;
using NovaVoice.Models;

namespace NovaVoice.Speech.Recorder;

public class PicovoiceRecorder : BaseRecorder
{
    private readonly ILogger<PicovoiceRecorder> _logger;
    private readonly Configuration _configuration;
    private readonly PvRecorder _recorder;
    
    public PicovoiceRecorder(ILogger<PicovoiceRecorder> logger, Configuration configuration) : base(logger, configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _recorder = PvRecorder.Create(frameLength: _configuration.FrameLength);
        
        _logger.LogInformation("Using Picovoice for Recording");
    }

    public override void StartRecording(CancellationToken cancellationToken = default)
    {
        if (!_recorder.IsRecording)
            _recorder.Start();
        
        // Start reading data in a background thread
        Task.Run(ReadData, cancellationToken); 
    }
    
    private void ReadData()
    {
        try
        {
            while (_recorder.IsRecording)
            {
                var pcmFrame = _recorder.Read();
                RaisePcmFrameReceived(pcmFrame);
            }
        }
        catch (OperationCanceledException)
        {
            // Recording has been stopped
        }
    }

    public override void StopRecording()
    {
        if (!_recorder.IsRecording) return;
        _recorder.Stop();
    }
    
    public override void Dispose()
    {
        StopRecording();
        _recorder.Dispose();
    }
}