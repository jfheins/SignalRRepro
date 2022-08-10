using Microsoft.AspNetCore.SignalR;
using SignalRRepro.Hubs;
using System.Diagnostics;
using System.Threading.Channels;

namespace SignalRRepro;

public class MessageSender : BackgroundService, IDisposable
{
    private readonly byte[] _dummyData = new byte[2048];
    private readonly IHubContext<MyHub>? _hub;
    private readonly Channel<Message> _channel;
    private readonly Task _consumeTask;

    public MessageSender(IHubContext<MyHub> hub)
    {
        _hub = hub;
        Random.Shared.NextBytes(_dummyData);
        _channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(5) { FullMode = BoundedChannelFullMode.DropOldest }, item =>
        {
            // Console.WriteLine($"Dropped: {item}");
        });
        _consumeTask = Consume();
    }

    private async Task Consume()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        await foreach (var msg in _channel.Reader.ReadAllAsync())
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var task = _hub!.Clients.All.SendAsync("clock", msg, cts.Token);
                bool delay = !task.IsCompletedSuccessfully;
                await task;
                sw.Stop();
                if (delay)
                {
                    Console.WriteLine($"Took {sw.ElapsedMilliseconds}ms, Buffer={_channel.Reader.Count}, Queue Delay={DateTime.UtcNow.Subtract(msg.SentTime).TotalMilliseconds}ms");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelled send");
            }

            // Reuse the cancellation token as well as it hasn't timed out
            if (!cts.TryReset())
            {
                cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(5));
        long count = 0;

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var dt = DateTime.UtcNow;
            var message = new Message { SentTime = dt, time = dt.ToString("O"), id = count++, data = _dummyData };
            _channel.Writer.TryWrite(message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();

        await base.StopAsync(cancellationToken);

        await _consumeTask;
    }

    private class Message
    {
        internal DateTime SentTime { get; set; }
        public string time { get; init; } = default!;
        public long id { get; init; }
        public byte[] data { get; init; } = default!;
    }
}