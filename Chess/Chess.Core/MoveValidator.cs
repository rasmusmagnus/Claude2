using Events.Commands;

namespace Chess.Core;

public class MoveValidator : IMoveValidator
{
    public bool Validate(MakeMoveCommand command, Board board)
    {
        if (!board.TryGetPieceAtPosition(command.Move.From, out var piece))
            return false;
        
        var possibleMoves = piece.GetPossibleMoves(command.Move.From);
        if (possibleMoves.Contains(command.Move.To))
            return true;
        return false;
    }
}