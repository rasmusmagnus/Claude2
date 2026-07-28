using Events;

namespace Chess.Core.Pieces
{
    internal class Knight : ChessPiece
    {
        public Knight(Colour colour) : base(colour)
        {
        }

        public override string ToFenCharecter()
        {
            return colour == Colour.White ? "N" : "n";
        }

        public override HashSet<Position> GetPossibleMoves(Position startPosition)
        {
            var res = new HashSet<Position>();
            var fileAsIndex = startPosition.FileToIndex();
            var rankAsIndex = startPosition.Rank;
            for (var sign1 = -1; sign1 <= 1; sign1 += 2)
            {
                for (var sign2 = -1; sign2 <= 1; sign2 += 2)
                {
                    Position.AddIfInBounds(rankAsIndex + 1 * sign1, fileAsIndex + 2 * sign2, res);        
                    Position.AddIfInBounds(rankAsIndex + 2 * sign1, fileAsIndex + 1 * sign2, res);        
                }
            }
            return res;
        }
    }
}