using Chess.Core.Pieces;
using Events.Commands;

namespace Chess.Core;

public class MoveValidator : IMoveValidator
{
    public bool Validate(MakeMoveCommand command, ChessPiece piece, Board board, bool isQueenSideCastlingMove,
        bool isKingSideCastlingMove)
    {
        var possibleMoves = piece.GetPossibleMoves(command.Move.From);
        if (!possibleMoves.Contains(command.Move.To))
            return false;

        if (IsNotMovingColoursTurn(command, board, piece))
            return false;
        if (IsTakingOwnPiece(command, board, piece))
            return false;
        if (!IsLegalCastlingMove(command, board, piece, isQueenSideCastlingMove, isKingSideCastlingMove))
            return false;
        if (IsMakingEnPassantMove(command, board, piece))
            return false;
        
        if (IsPieceInTheWay(command, board, piece))
            return false;
        if (LeavesMovingColourInCheck(command, board, piece))
            return false;
        if (IsPromotingMove(command, board, piece))
            return false;
        
        return true;
    }

    private static bool IsNotMovingColoursTurn(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        return piece.colour != board.MovingColour;
    }

    private static bool IsLegalCastlingMove(MakeMoveCommand command, Board board, ChessPiece piece, bool isQueenSideCastlingMove, bool isKingSideCastlingMove)
    {
        if (piece is not King)
            return true;
        var startPos = piece.colour == Colour.White ? King.WhiteStartPosition : King.BlackStartPosition;
        if (command.Move.From != startPos)
        {
            return true;
        }
        if (!isQueenSideCastlingMove && !isKingSideCastlingMove)
            return true;
        var state = piece.colour == Colour.White ? board.WhiteCastlingState : board.BlackCastlingState;
        if (isKingSideCastlingMove && state.KingsideAvailable)
            return true;
        if (isQueenSideCastlingMove && state.QueenSideAvailable)
            return true;
        //TODO
        var isCastlingSquaresUnderAttack = true;
        if (!isCastlingSquaresUnderAttack)
            return true;
        return false;
    }

    private static bool IsPromotingMove(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        return false;
    }

    private static bool IsMakingEnPassantMove(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        return false;
    }

    private static bool LeavesMovingColourInCheck(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        return false;
    }

    private static bool IsPieceInTheWay(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        return false;
    }

    private static bool IsTakingOwnPiece(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        if (!board.TryGetPieceAtPosition(command.Move.To, out var pieceAtTo)) return false;
        return pieceAtTo.colour == piece.colour;
    }
}