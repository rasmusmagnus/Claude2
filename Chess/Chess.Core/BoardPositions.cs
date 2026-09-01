using System.Diagnostics;
using Chess.Core.Pieces;
using Events;

namespace Chess.Core
{
    public class BoardPositions
    {
        private readonly ChessPiece?[][] _state;

        private BoardPositions(ChessPiece?[][] state)
        {
            this._state = state;
        }

        public ChessPiece? this[Position position]
        {
            get
            {
                var (indexFinal, letterIndex) = GetIndexFromNotation(position);

                return _state[indexFinal][letterIndex];
            }
            private set
            {
                var (indexFinal, letterIndex) = GetIndexFromNotation(position);
                _state[indexFinal][letterIndex] = value;
            }
        }

        private static (int, int) GetIndexFromNotation(Position position)
        {
            int indexInt = position.File switch
            {
                'a' => 0,
                'b' => 1,
                'c' => 2,
                'd' => 3,
                'e' => 4,
                'f' => 5,
                'g' => 6,
                'h' => 7,
                _ => throw new ArgumentException("Invalid index")
            };
            return (position.Rank - 1, indexInt);
        }

        public BoardPositions(string fenString)
        {
            var result = new ChessPiece?[8][];
            var trimmedFen = fenString.Split(" ")[0];

            var fenRows = trimmedFen.Split("/");

            for (int i = 0; i < fenRows.Length; i++)
            {
                var file = fenRows[i];
                result[7 - i] = new ChessPiece?[8];
                var squareIndex = 0;
                for (int j = 0; j < file.Length; j++)
                {
                    var square = file[j];


                    if (int.TryParse(square.ToString(), out var count))
                    {
                        for (int k = 0; k < count; k++)
                        {
                            result[7 - i][squareIndex] = null;
                            squareIndex++;
                        }
                    }
                    else
                    {
                        var colour = square.ToString().ToLower().Equals(square.ToString())
                            ? Colour.Black
                            : Colour.White;
                        ChessPiece piece;
                        switch (square.ToString().ToLower())
                        {
                            case "r":
                                piece = new Rook(colour); break;
                            case "p":
                                piece = new Pawn(colour); break;
                            case "b":
                                piece = new Bishop(colour); break;
                            case "n":
                                piece = new Knight(colour); break;
                            case "k":
                                piece = new King(colour); break;
                            case "q":
                                piece = new Queen(colour); break;
                            default:
                                throw new Exception("wfts");
                        }

                        result[7 - i][squareIndex] = piece;
                        squareIndex++;
                    }
                }
            }

            _state = result;
        }

        public string GetPiecesFenPart()
        {
            var result = "";
            foreach (var file in _state.Reverse())
            {
                var counter = 0;
                foreach (var square in file)
                {
                    if (square != null)
                    {
                        if (counter != 0)
                        {
                            result += counter;
                            counter = 0;
                        }

                        result += square.ToFenCharecter();
                    }
                    else
                    {
                        counter++;
                    }
                }

                if (counter != 0)
                {
                    result += counter;
                }

                result += "/";
            }

            return result[..^1];
        }

        public void MovePieces(IChessMove move)
        {
            var fromPiece = this[move.From];
            Debug.Assert(fromPiece != null);
            this[move.From] = null;
            this[move.To] = fromPiece;
            Debug.Assert(this[move.From] == null);
        }

        public void MovePiecesUnderEnPassantAttack(IChessMove move)
        {
            var from = move.From;
            var to = move.To;
            var dir = from - to;
            var unitDir = (0, dir.Item2 / Math.Abs(dir.Item2));
            var targetPawnPos = to + unitDir;
            this[targetPawnPos] = null;
            MovePieces(move);
        }

        public void MovePiecesForCastling(IChessMove move, bool isQueenSideCastlingMove, bool isKingSideCastlingMove)
        {
            var kingPos = move.From;
            var king = this[kingPos];
            Debug.Assert(king != null);
            Debug.Assert(king is King);
            var rookFile = isQueenSideCastlingMove ? 'a' : 'h';
            var rookPos = new Position(rookFile, move.From.Rank);
            var rook = this[rookPos];
            Debug.Assert(rook != null);
            Debug.Assert(rook is Rook);
            var rookToPositionFile = isQueenSideCastlingMove ? 'c' : 'f';
            var kingToPositionFile = isQueenSideCastlingMove ? 'b' : 'g';
            var rookToRank = move.From.Rank;
            var kingToRank = move.From.Rank;
            var kingToPos = new Position(kingToPositionFile, kingToRank);
            var rookToPos = new Position(rookToPositionFile, rookToRank);
            this[kingToPos] = king;
            this[rookToPos] = rook;
            this[rookPos] = null;
            this[kingPos] = null;
            Debug.Assert(this[move.From] == null);
        }

        public BoardPositions Copy()
        {
            var outer = new ChessPiece?[_state.Length][];
            var i = 0;
            foreach (var fileOrRank in _state)
            {
                var copied = new ChessPiece?[_state.Length];
                fileOrRank.CopyTo(copied, 0);
                outer[i] = copied;
                i++;
            }

            return new BoardPositions(outer);
        }

        public Position GetKingPosition(Colour pieceColour)
        {
            var reverse = _state;
            for (int file = 0; file < _state.Length; file++)
            {
                for (int rank = 0; rank < _state.Length; rank++)
                {
                    var piece = reverse[rank][file];
                    if (piece is King king && king.Colour == pieceColour)
                        return new Position(file + 1, rank + 1);
                }
            }

            throw new Exception("No king was found!");
        }

        public ChessPiece? GetFirstPieceAtEastRank(Position kingPos)
        {
            var indexShift = 1;
            while (!Position.IsOutOfBounds(kingPos.FileToIndex() + indexShift, kingPos.Rank))
            {
                var resultingPos = new Position(kingPos.FileToIndex() + indexShift, kingPos.Rank);
                var piece = this[resultingPos];
                if (piece != null)
                    return piece;
                indexShift++;
            }

            return null;
        }
        
        public ChessPiece? GetFirstPieceAtWestRank(Position kingPos)
        {
            var indexShift = 1;
            while (!Position.IsOutOfBounds(kingPos.FileToIndex() - indexShift, kingPos.Rank))
            {
                var resultingPos = new Position(kingPos.FileToIndex() - indexShift, kingPos.Rank);
                var piece = this[resultingPos];
                if (piece != null)
                    return piece;
                indexShift++;
            }

            return null;
        }

        public ChessPiece? GetFirstPieceAtSouthFile(Position kingPos)
        {
            var indexShift = 1;
            while (!Position.IsOutOfBounds(kingPos.FileToIndex(), kingPos.Rank - indexShift))
            {
                var resultingPos = new Position(kingPos.FileToIndex(), kingPos.Rank - indexShift);
                var piece = this[resultingPos];
                if (piece != null)
                    return piece;
                indexShift++;
            }

            return null;
        }

        public ChessPiece? GetFirstPieceAtNorthFile(Position kingPos)
        {
            var indexShift = 1;
            while (!Position.IsOutOfBounds(kingPos.FileToIndex(), kingPos.Rank + indexShift))
            {
                var resultingPos = new Position(kingPos.FileToIndex(), kingPos.Rank + indexShift);
                var piece = this[resultingPos];
                if (piece != null)
                    return piece;
                indexShift++;
            }

            return null;
        }

        public ChessPiece? GetFirstPieceAtNorthEastDiagonal(Position kingPos)
        {
            var indexShift = 1;
            while (!Position.IsOutOfBounds(kingPos.FileToIndex() + indexShift, kingPos.Rank + indexShift))
            {
                var resultingPos = new Position(kingPos.FileToIndex() + indexShift, kingPos.Rank + indexShift);
                var piece = this[resultingPos];
                if (piece != null)
                    return piece;
                indexShift++;
            }

            return null;
        }

        public ChessPiece? GetFirstPieceAtSouthEastDiagonal(Position kingPos)
        {
            var indexShift = 1;
            while (!Position.IsOutOfBounds(kingPos.FileToIndex() + indexShift, kingPos.Rank - indexShift))
            {
                var resultingPos = new Position(kingPos.FileToIndex() + indexShift, kingPos.Rank - indexShift);
                var piece = this[resultingPos];
                if (piece != null)
                    return piece;
                indexShift++;
            }

            return null;
        }

        public ChessPiece? GetFirstPieceAtSouthWestDiagonal(Position kingPos)
        {
            var indexShift = 1;
            while (!Position.IsOutOfBounds(kingPos.FileToIndex() - indexShift, kingPos.Rank - indexShift))
            {
                var resultingPos = new Position(kingPos.FileToIndex() - indexShift, kingPos.Rank - indexShift);
                var piece = this[resultingPos];
                if (piece != null)
                    return piece;
                indexShift++;
            }

            return null;
        }

        public ChessPiece? GetFirstPieceAtNorthWestDiagonal(Position kingPos)
        {
            var indexShift = 1;
            while (!Position.IsOutOfBounds(kingPos.FileToIndex() - indexShift, kingPos.Rank + indexShift))
            {
                var resultingPos = new Position(kingPos.FileToIndex() - indexShift, kingPos.Rank + indexShift);
                var piece = this[resultingPos];
                if (piece != null)
                    return piece;
                indexShift++;
            }

            return null;
        }

        public Knight?[] GetAttackingKnights(Position kingPos)
        {
            var res = new List<Knight?>();
            var fakeKnight = new Knight(Colour.Black);
            var posKnightPositions = fakeKnight.GetPossibleMoves(kingPos);

            foreach (var pos in posKnightPositions)
            {
                if(this[pos] is Knight knight)
                    res.Add(knight);
            }
            return res.ToArray();
        }

        public King? GetAttackingKing(Position kingPos)
        {
            var fakeKing = new King(Colour.Black);
            var posKingPositions = fakeKing.GetPossibleMoves(kingPos);
            foreach (var pos in posKingPositions)
            {
                if (this[pos] is King king)
                    return king;
            }
            return null;
        }
        
        public Pawn?[] GetAttackingPawns(Position kingPos, Colour kingColour)
        {
            var res = new List<Pawn?>();
            var fakePawn = new Pawn(kingColour);
            var posPawnPositions = fakePawn.GetDiagonalMoves(kingPos);
            foreach (var pos in posPawnPositions)
            {
                if (this[pos] is Pawn pawn)
                    res.Add(pawn);
            }
            return res.ToArray();
        }
    }
}