using System.Threading.Channels;

namespace Events;

public class EventDistributor<T> : IEventProducer<T>, IEventConsumer<T> where T : class 
{
    private readonly Channel<T> _channel;

    public EventDistributor()
    {
        _channel = Channel.CreateUnbounded<T>();
    }

    public void SubmitEvent(T evt)
    {
        _channel.Writer.TryWrite(evt);
    }

    public ChannelReader<T> GetReader()
    {
        return _channel.Reader;
    }
}