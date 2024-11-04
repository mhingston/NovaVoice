using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace NovaVoice;

public static class SoundPlayer
{
    public static async Task PlayAsync(string fileName, ILogger? logger = null)
    {
        try
        {
            await using var audioFile = new AudioFileReader(Path.Join(Directory.GetCurrentDirectory(), "Sounds", fileName));
            using var outputDevice = new WaveOutEvent();

            outputDevice.Init(audioFile);
            
            using var cts = new CancellationTokenSource();
            var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            outputDevice.PlaybackStopped += (_, _) =>
            {
                tcs.TrySetResult();
                cts.Cancel();
            };

            outputDevice.Play();
            
            await Task.WhenAny(tcs.Task, Task.Delay(-1, cts.Token));
        }
        
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error playing sound {FileName}", fileName);
        }
    }
}