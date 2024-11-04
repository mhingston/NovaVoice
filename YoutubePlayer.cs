using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace NovaVoice;

public class YoutubePlayer : IDisposable
{
    private readonly ILogger<YoutubePlayer> _logger;
    private Process? _ytdlpProcess;
    private Process? _mpvProcess;
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    public YoutubePlayer(ILogger<YoutubePlayer> logger)
    {
        _logger = logger;
    }
    
    public async Task PlayAsync(string url)
    {
        await _lock.WaitAsync();
        
        try
        {
            _ytdlpProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"-f ba --extract-audio -x \"{url}\" -o -",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            
            _mpvProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "mpv",
                    Arguments = "-",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };
            
            _ytdlpProcess.Start();
            _mpvProcess.Start();
        }
        
        finally
        {
            _lock.Release();
        }

        try
        {
            await _ytdlpProcess.StandardOutput.BaseStream.CopyToAsync(_mpvProcess.StandardInput.BaseStream);
            _mpvProcess.StandardInput.Close();
            await _mpvProcess.WaitForExitAsync();
        }
        
        catch (Exception ex) when (ex is InvalidOperationException or IOException)
        {
            _logger.LogWarning("Playback stopped");
        }
        
        finally
        {
            await StopAsync();
        }
    }

    public async Task StopAsync()
    {
        await _lock.WaitAsync();
        
        try
        {
            if (_ytdlpProcess is { HasExited: false })
            {
                try
                {
                    _ytdlpProcess.Kill(true);
                }
                
                catch (InvalidOperationException)
                {
                    // Process was already terminated
                }
            }

            if (_mpvProcess is { HasExited: false })
            {
                try
                {
                    _mpvProcess.Kill(true);
                }
                
                catch (InvalidOperationException)
                {
                    // Process was already terminated
                }
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        _lock.Wait();
        
        try
        {
            if (_ytdlpProcess != null)
            {
                try
                {
                    if (!_ytdlpProcess.HasExited)
                    {
                        _ytdlpProcess.Kill(true);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Process was already terminated
                }
                _ytdlpProcess.Dispose();
            }

            if (_mpvProcess != null)
            {
                try
                {
                    if (!_mpvProcess.HasExited)
                    {
                        _mpvProcess.Kill(true);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Process was already terminated
                }
                _mpvProcess.Dispose();
            }

            _lock.Dispose();
        }
        finally
        {
            _lock.Release();
        }
    }
}