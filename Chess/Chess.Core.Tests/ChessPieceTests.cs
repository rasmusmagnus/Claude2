using Chess.Core.Pieces;
using Events;

namespace Chess.Core.Tests;

public class ChessPieceTests
{
    [Theory()]
    [InlineData('a', 1)]
    [InlineData('a', 8)]
    [InlineData('h', 1)]
    [InlineData('h', 8)]
    public void KingInCornerReturnsCorrectMoves(char file, char rank)
    {
        var pos = new Position(file, rank);
        var king = new King(Colour.White);
        var moves = king.GetPossibleMoves(pos);
        
        Assert.Equal(3, moves.Count());
    }
}