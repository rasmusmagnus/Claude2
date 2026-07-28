using Chess.Core.Pieces;
using Events.Commands;

namespace Chess.Core;

public class AllOkMoveValidator : IMoveValidator
{
    public bool Validate(MakeMoveCommand command, ChessPiece piece, Board board, bool isQueenSideCastlingMove,
        bool isCastlingMove)
    {
        return true;
    }
}