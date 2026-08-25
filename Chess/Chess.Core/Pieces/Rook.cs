using Events;

namespace Chess.Core.Pieces
{
    internal class Rook : ChessPiece
    {
        public Rook(Colour colour) : base(colour)
        {
        }

        public override string ToFenCharecter()
        {
            return Colour == Colour.White ? "R" : "r";
        }

        public override HashSet<Position> GetPossibleMoves(Position startPosition)
        {
            var res = new HashSet<Position>();
            var fileAsIndex = startPosition.FileToIndex();
            var rankAsIndex = startPosition.Rank;

            for (var sign1 = -1; sign1 <= 1; sign1 += 2)
            {
                var i = 1;
                while (!Position.IsOutOfBounds(rankAsIndex, fileAsIndex + sign1 * i))
                {
                    res.Add(new Position(fileAsIndex + sign1 * i, rankAsIndex));
                    i++;
                }
            }

            for (var sign2 = -1; sign2 <= 1; sign2 += 2)
            {
                var i = 1;
                while (!Position.IsOutOfBounds(rankAsIndex + sign2 * i, fileAsIndex))
                {
                    res.Add(new Position(fileAsIndex, rankAsIndex + sign2 * i));
                    i++;
                }
            }

            return res;
        }
    }
}