using System.Diagnostics;

namespace SignalRRepro;

public class Logger : IHostedService, IDisposable
{
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(Log, null, TimeSpan.FromSeconds(0.5),
            TimeSpan.FromSeconds(1));
        return Task.CompletedTask;
    }

    private void Log(object? _)
    {
        var usedMiB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024;
        Console.WriteLine($"Using {usedMiB:F1} MiB");
    }
    
    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}