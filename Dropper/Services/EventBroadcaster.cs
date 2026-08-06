using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Dropper.Services;

public class EventBroadcaster<TEvent>
{
    private readonly ConcurrentDictionary<ChannelWriter<TEvent>, byte> _subscribers = new();

    public void Subscribe(ChannelWriter<TEvent> subscriber, CancellationToken cancellationToken)
    {
        _subscribers.TryAdd(subscriber, 0);
        cancellationToken.Register(() => Unsubscribe(subscriber));
    }

    private void Unsubscribe(ChannelWriter<TEvent> subscriber)
    {
        _subscribers.TryRemove(subscriber, out _);
    }

    public async Task PublishAsync(TEvent @event)
    {
        foreach (var (subscriber, _) in _subscribers)
        {
            await subscriber.WriteAsync(@event);
        }
    }
}