using System.Threading.Channels;
using Chess.Core.Pieces;
using Chess.Shared;
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

    [Fact]
    public async Task PromotionWaitsForChoiceAndPublishesCompletedPosition()
    {
        var commands = new EventDistributor<ICommand>();
        var events = new EventDistributor<IGameEvent>();
        var board = new Board(new AllOkMoveValidator(), events, commands,
            "7k/P7/8/8/8/8/8/7K w - - 0 1");
        using var cancellation = new CancellationTokenSource();
        var runTask = board.RunAsync(cancellation.Token);
        var eventReader = events.GetReader();

        Assert.IsType<BoardUpdateEvent>(await eventReader.ReadAsync());
        commands.SubmitEvent(new MakeMoveCommand(new Move(new Position('a', 7), new Position('a', 8))));

        var promoting = Assert.IsType<PromotingEvent>(await eventReader.ReadAsync());
        Assert.Equal(Colour.White, promoting.Colour);
        Assert.Equal(new Position('a', 8), promoting.Pos);
        Assert.Single(board.stateHistory);

        commands.SubmitEvent(new PromoteToCommand(PromoteOptions.Queen));

        var update = Assert.IsType<BoardUpdateEvent>(await eventReader.ReadAsync());
        Assert.StartsWith("Q6k/8/8/8/8/8/8/7K b", update.BoardFenNotation);
        Assert.Equal(2, board.stateHistory.Count);
        Assert.Equal(update.BoardFenNotation, board.stateHistory[^1]);

        cancellation.Cancel();
        try
        {
            await runTask;
        }
        catch (OperationCanceledException)
        {
            // Cancellation may be observed by the loop condition or the pending channel read.
        }
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