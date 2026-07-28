using System.Threading.Channels;
using Events;
using Events.Commands;
using Events.Events;
using Xunit.Sdk;

namespace Chess.Core.Tests;

public class BoardTests
{
    private static IMoveValidator _moveValidator = new AllOkMoveValidator();
    private static IEventConsumer<ICommand> _consumer = new ConsumerFixture();
    private static IEventProducer<IGameEvent> _producer = new ProducerFixture();
    
    private static readonly TestOutputHelper _testOutputHelper = new ();
    
    [Fact]
    public void TestNoMoveFen()
    {
        var board = new Board(_moveValidator, _producer, _consumer, Board.StartBoardFen);
        var res = board.ToFen();
        Assert.Equal(Board.StartBoardFen, res);
    }
    
    [Theory]
    [InlineData("2Q3k1/ppp1p1b1/1q1p4/5pNn/P2P4/4P3/4KPP1/8 w - - 0 1")]
    public void FlushTest(string fen)
    {
        var board = new Board(new MoveValidator(), _producer, _consumer, fen);
        var move = new Move(new ('c', 8), new ('g', 8));
        board.HandleMoveCommand(new MakeMoveCommand(move));
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