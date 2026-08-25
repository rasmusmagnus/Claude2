using Chess.Core.Pieces;
using Events;
using Events.Commands;

namespace Chess.Core;

public class MoveValidator : IMoveValidator
{
    
    private static Type[] _diagonalAttackPieces = [typeof(Queen), typeof(Bishop)];
    private static Type[] _cardinalAttackPieces = [typeof(Queen), typeof(Rook)];
    private static Type[] _knightAttackPieces = [typeof(Knight)];
    private static Type[] _kingAttackPieces = [typeof(King)];
    private static Type[] _pawnAttackPieces = [typeof(Pawn)];
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
        if (IsIllegalCastlingMove(command, board, piece, isQueenSideCastlingMove, isKingSideCastlingMove))
            return false;
        if (IsMakingEnPassantMove(command, board, piece))
            return false;
        if (IsPieceInTheWay(command, board, piece))
            return false;
        if (IsPromotingMove(command, board, piece))
            return false;
        if (LeavesMovingColourInCheck(command, board, piece))
            return false;
        
        return true;
    }

    private static bool IsNotMovingColoursTurn(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        return piece.Colour != board.MovingColour;
    }

    private static bool IsIllegalCastlingMove(MakeMoveCommand command, Board board, ChessPiece piece, bool isQueenSideCastlingMove, bool isKingSideCastlingMove)
    {
        if (piece is not King)
            return false;
        var startPos = piece.Colour == Colour.White ? King.WhiteStartPosition : King.BlackStartPosition;
        if (command.Move.From != startPos)
        {
            return false;
        }
        if (!isQueenSideCastlingMove && !isKingSideCastlingMove)
            return false;
        var state = piece.Colour == Colour.White ? board.WhiteCastlingState : board.BlackCastlingState;
        
        if (isKingSideCastlingMove && !state.KingsideAvailable)
            return true;
        if (isQueenSideCastlingMove && !state.QueenSideAvailable)
            return true;
        var castlingSquares = isKingSideCastlingMove
            ? CastlingState.KingSideCastlingSquares(piece.Colour)
            : CastlingState.QueenSideCastlingSquares(piece.Colour);
        foreach (var pos in castlingSquares)
        {
            if(IsBoardInCheck(pos, piece.Colour, board.Positions))
                return true;
        }
        if(IsBoardInCheck(board.Positions.GetKingPosition(piece.Colour), piece.Colour, board.Positions))
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
        var futureBoard = board.Positions.Copy();
        futureBoard.MovePieces(command.Move);
        var kingPos = futureBoard.GetKingPosition(piece.Colour);

        if (IsBoardInCheck(kingPos, piece.Colour, futureBoard)) 
            return true;

        return false;
    }

    private static bool IsBoardInCheck(Position kingPos, Colour kingColour, BoardPositions board)
    {
        var oppositeColour = kingColour == Colour.White ? Colour.Black : Colour.White;

        if (HasOppositeAttacker(board.GetFirstPieceAtEastRank(kingPos), oppositeColour, _cardinalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtSouthFile(kingPos), oppositeColour, _cardinalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtNorthEastDiagonal(kingPos), oppositeColour, _diagonalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtSouthEastDiagonal(kingPos), oppositeColour, _diagonalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtSouthWestDiagonal(kingPos), oppositeColour, _diagonalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtNorthWestDiagonal(kingPos), oppositeColour, _diagonalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtWestRank(kingPos), oppositeColour, _cardinalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtNorthFile(kingPos), oppositeColour, _cardinalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetAttackingKing(kingPos), oppositeColour, _kingAttackPieces))
            return true;
        if (board.GetAttackingKnights(kingPos).Any(k => HasOppositeAttacker(k, oppositeColour, _knightAttackPieces)))
            return true;
        if (board.GetAttackingPawns(kingPos, kingColour).Any(k => HasOppositeAttacker(k, oppositeColour, _pawnAttackPieces)))
            return true;

        return false;
    }

    private static bool HasOppositeAttacker(ChessPiece? attackingPiece, Colour oppositeColour, Type[] attackingTypes)
    {
        return attackingPiece != null && attackingPiece.Colour == oppositeColour && attackingTypes.Contains(attackingPiece.GetType());
    }

    private static bool IsPieceInTheWay(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        return false;
    }

    private static bool IsTakingOwnPiece(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        if (!board.TryGetPieceAtPosition(command.Move.To, out var pieceAtTo)) return false;
        return pieceAtTo.Colour == piece.Colour;
    }
}