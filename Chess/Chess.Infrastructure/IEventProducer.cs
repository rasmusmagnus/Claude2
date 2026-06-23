namespace Events;

public interface IEventProducer<T> where T : class
{
    public void SubmitEvent(T evt);
}