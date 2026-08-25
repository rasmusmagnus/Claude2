using Events;

namespace Chess.Core.Pieces
{
	internal class Bishop : ChessPiece
	{
		public Bishop(Colour colour) : base(colour)
		{
		}

		public override string ToFenCharecter()
		{
			return Colour == Colour.White ? "B" : "b";
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
					var i = 1;
					while (!Position.IsOutOfBounds(rankAsIndex+sign2* i, fileAsIndex+sign1 *i))
					{
						res.Add(new Position(fileAsIndex+sign1 *i, rankAsIndex+sign2*i));
						i++;
					}
				}
			}
			return res;

		}
	}
}
