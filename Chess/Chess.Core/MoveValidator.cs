using Chess.Core.Pieces;
using Events;
using Events.Commands;

namespace Chess.Core;

public class MoveValidator : IMoveValidator
{
    private static readonly Type[] DiagonalAttackPieces = [typeof(Queen), typeof(Bishop)];
    private static readonly Type[] CardinalAttackPieces = [typeof(Queen), typeof(Rook)];
    private static readonly Type[] KnightAttackPieces = [typeof(Knight)];
    private static readonly Type[] KingAttackPieces = [typeof(King)];
    private static readonly Type[] PawnAttackPieces = [typeof(Pawn)];

    public bool Validate(MakeMoveCommand command, ChessPiece piece, Board board, bool isQueenSideCastlingMove,
        bool isKingSideCastlingMove)
    {
        var possibleMoves = piece.GetPossibleMoves(command.Move.From);

        if (!possibleMoves.Contains(command.Move.To))
            return false;


        if (IsNotViablePieceMove(command, board, piece))
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

    private static bool IsNotViablePieceMove(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        var isPieceBetween = board.IsPieceBetween(command.Move);
        if (piece is Knight)
            return false;
        if (piece is Pawn pawn && !isPieceBetween)
        {
            var dir = command.Move.To - command.Move.From;
            var isCapturing = (board.Positions[command.Move.To] is { } targetPiece &&
                               piece.Colour != targetPiece.Colour);
            ;
            if (command.Move.To == board.GetEnPassantSquare() || (isCapturing && dir.Item1 != 0))
                return false;
            if (!isCapturing && dir.Item1 != 0)
                return true;

            return isCapturing && dir.Item1 == 0;
        }

        return isPieceBetween;
    }


    private static bool IsNotMovingColoursTurn(MakeMoveCommand command, Board board, ChessPiece piece)
    {
        return piece.Colour != board.MovingColour;
    }

    private static bool IsIllegalCastlingMove(MakeMoveCommand command, Board board, ChessPiece piece,
        bool isQueenSideCastlingMove, bool isKingSideCastlingMove)
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
            if (IsBoardInCheck(pos, piece.Colour, board.Positions))
                return true;
        }

        if (IsBoardInCheck(board.Positions.GetKingPosition(piece.Colour), piece.Colour, board.Positions))
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

    public static bool IsBoardInCheck(Position kingPos, Colour kingColour, BoardPositions board)
    {
        var oppositeColour = kingColour == Colour.White ? Colour.Black : Colour.White;

        if (HasOppositeAttacker(board.GetFirstPieceAtEastRank(kingPos), oppositeColour, CardinalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtSouthFile(kingPos), oppositeColour, CardinalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtNorthEastDiagonal(kingPos), oppositeColour, DiagonalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtSouthEastDiagonal(kingPos), oppositeColour, DiagonalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtSouthWestDiagonal(kingPos), oppositeColour, DiagonalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtNorthWestDiagonal(kingPos), oppositeColour, DiagonalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtWestRank(kingPos), oppositeColour, CardinalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetFirstPieceAtNorthFile(kingPos), oppositeColour, CardinalAttackPieces))
            return true;
        if (HasOppositeAttacker(board.GetAttackingKing(kingPos), oppositeColour, KingAttackPieces))
            return true;
        if (board.GetAttackingKnights(kingPos).Any(k => HasOppositeAttacker(k, oppositeColour, KnightAttackPieces)))
            return true;
        if (board.GetAttackingPawns(kingPos, kingColour)
            .Any(k => HasOppositeAttacker(k, oppositeColour, PawnAttackPieces)))
            return true;

        return false;
    }

    private static bool HasOppositeAttacker(ChessPiece? attackingPiece, Colour oppositeColour, Type[] attackingTypes)
    {
        return attackingPiece != null && attackingPiece.Colour == oppositeColour &&
               attackingTypes.Contains(attackingPiece.GetType());
    }

    public static bool IsKingInCheckMate(Position kingPos, Colour kingColour, Board board)
    {
        var allyPiecePositions = board.Positions.GetColourPiecePositions(kingColour);

        foreach (var pos in allyPiecePositions)
        {
            var piece = board.Positions[pos];

            var moves = piece!.GetPossibleMoves(pos);
            foreach (var move in moves)
            {
                var moveCommand = new MakeMoveCommand(new Move(pos, move));
                if (IsNotViablePieceMove(moveCommand, board, piece))
                {
                    continue;
                }

                if (!LeavesMovingColourInCheck(moveCommand, board, piece))
                    return false;
            }
        }

        return true;
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