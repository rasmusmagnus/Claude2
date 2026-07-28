using Chess.Core.Pieces;
using Events.Commands;

namespace Chess.Core;

public interface IMoveValidator
{
    bool Validate(MakeMoveCommand command, ChessPiece piece, Board board, bool isQueenSideCastlingMove,
        bool isKingCastlingMove);
}