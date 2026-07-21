using System.Threading.Channels;
using Events;
using Events.Commands;
using Events.Events;

namespace Chess.Core.Tests;

public class BoardTests
{
    private static IMoveValidator _moveValidator = new AllOkMoveValidator();
    private static IEventConsumer<ICommand> _consumer = new ConsumerFixture();
    private static IEventProducer<IGameEvent> _producer = new ProducerFixture();
    
    [Fact]
    public void TestNoMoveFen()
    {
        var board = new Board(_moveValidator, _producer, _consumer);
        var res = board.ToFen();
        Assert.Equal(Board.StartBoardFen, res);
    }

    private class ConsumerFixture() : IEventConsumer<ICommand>
    {
        public ChannelReader<ICommand> GetReader()
        {
            return Channel.CreateUnbounded<ICommand>();
        }
    }
    
    private class ProducerFixture() : IEventProducer<IGameEvent>
    {
        public void SubmitEvent(IGameEvent evt)
        {
            return;
        }
    }
}