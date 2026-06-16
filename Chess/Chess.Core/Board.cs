using Events;
using Events.Events;

namespace Chess.Core;

public class Board
{
    private readonly IEventProducer<IGameEvent> _producer;
    private readonly IEventConsumer<IGameEvent> _consumer;

    public static string StartBoardFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    
    public Board(IEventProducer<IGameEvent> producer, IEventConsumer<IGameEvent> consumer)
    {
        _producer = producer;
        _consumer = consumer;
    }

    private Board(string producer)
    {
        throw new NotImplementedException();
    }

    public Board GetBoardFromFen(string fen)
    {
        return new Board(fen);
    }

    public void MakeMove(IChessMove move)
    {
    }

    public static string ToFen(Board board)
    {
        return StartBoardFen;
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