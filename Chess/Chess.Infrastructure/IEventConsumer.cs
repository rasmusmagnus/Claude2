using System.Threading.Channels;

namespace Events;

public interface IEventConsumer<T> where T : class
{
    public ChannelReader<T> GetReader();
}