using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using SignalRRepro.Hubs;

namespace SignalRRepro;

public class MessageSender : IHostedService, IDisposable
{
    private readonly IServiceProvider _sp;
    private Timer? _timer;
    
    private static readonly Random Rand = new();
    private readonly byte[] _dummyData = new byte[2048];
    private IHubContext<MyHub>? _hub;

    public MessageSender(IServiceProvider sp)
    {
        _sp = sp;
        Rand.NextBytes(_dummyData);
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(2.5),
            TimeSpan.FromMilliseconds(5));
        _hub = _sp.GetRequiredService<IHubContext<MyHub>>();

        return Task.CompletedTask;
    }

    private void DoWork(object? _)
    {
        var message = new { time = DateTime.UtcNow.ToString("O"), data = _dummyData };
        SendTime(message);
    }

    private async void SendTime(object msg)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        try
        {
            await _hub!.Clients.All.SendAsync("clock", msg, cts.Token);
        }
        catch (OperationCanceledException)
        {
            //Console.WriteLine("Cancelled send");
        }
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