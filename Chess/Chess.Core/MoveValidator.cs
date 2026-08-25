using Chess.Core.Pieces;
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
        if (!IsLegalCastlingMove(command, board, piece, isQueenSideCastlingMove, isKingSideCastlingMove))
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

    private static bool IsLegalCastlingMove(MakeMoveCommand command, Board board, ChessPiece piece, bool isQueenSideCastlingMove, bool isKingSideCastlingMove)
    {
        if (piece is not King)
            return true;
        var startPos = piece.Colour == Colour.White ? King.WhiteStartPosition : King.BlackStartPosition;
        if (command.Move.From != startPos)
        {
            return true;
        }
        if (!isQueenSideCastlingMove && !isKingSideCastlingMove)
            return true;
        var state = piece.Colour == Colour.White ? board.WhiteCastlingState : board.BlackCastlingState;
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
        var futureBoard = board.Positions.Copy();
        futureBoard.MovePieces(command.Move);
        if (IsBoardInCheck(piece, futureBoard)) 
            return true;

        return false;
    }

    private static bool IsBoardInCheck(ChessPiece piece, BoardPositions board)
    {
        var kingPos = board.GetKingPosition(piece.Colour);
        var oppositeColour = piece.Colour == Colour.White ? Colour.Black : Colour.White;

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
        if (board.GetAttackingPawns(kingPos, piece.Colour).Any(k => HasOppositeAttacker(k, oppositeColour, _pawnAttackPieces)))
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