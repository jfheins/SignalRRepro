using Microsoft.AspNetCore.SignalR;
using SignalRRepro.Hubs;

namespace SignalRRepro;

public class MessageSender : BackgroundService, IDisposable
{
    private readonly byte[] _dummyData = new byte[2048];
    private IHubContext<MyHub>? _hub;

    public MessageSender(IHubContext<MyHub> hub)
    {
        _hub = hub;
        Random.Shared.NextBytes(_dummyData);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var message = new { time = DateTime.UtcNow.ToString("O"), data = _dummyData };
            await SendTime(message);
        }
    }

    private async Task SendTime(object msg)
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
}