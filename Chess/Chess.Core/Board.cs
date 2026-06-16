using Events;
using Events.Events;

namespace Chess.Core;

public class Board
{
    private readonly IEventProducer<IBoardStateEvent> _producer;
    private readonly IEventConsumer<IBoardStateEvent> _consumer;

    public Board(IEventProducer<IBoardStateEvent> producer, IEventConsumer<IBoardStateEvent> consumer)
    {
        _producer = producer;
        _consumer = consumer;
    }

    public async Task RunAsync(CancellationToken token)
    {
        var evt = new BoardUpdateEvent();

        _producer.SubmitEvent(evt);

        var reader = _consumer.GetReader();

        while (!token.IsCancellationRequested)
        {
            var res = await reader.ReadAsync(token);
            switch(res)
            {
                case BoardUpdateEvent boardUpdateEvent:
                {
                    HandleBoardUpdateEvent(boardUpdateEvent);
                    break;
                }
            }
        }
    }

    private void HandleBoardUpdateEvent(BoardUpdateEvent boardUpdateEvent)
    {
        throw new NotImplementedException();
    }
}