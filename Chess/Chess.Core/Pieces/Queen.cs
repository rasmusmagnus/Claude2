using Events;

namespace Chess.Core.Pieces {
	public class Queen : ChessPiece {
		public Queen(Colour colour) : base(colour) {
		}

		public override string ToFenCharecter() {
			return colour == Colour.White ? "Q" : "q";
		}

		public override HashSet<Position> GetPossibleMoves(Position startPosition)
		{
			var res = new HashSet<Position>();
			var fileAsIndex = startPosition.FileToIndex();
			var rankAsIndex = startPosition.Rank;

			for (var sign1 = -1; sign1 <= 1; sign1 += 2)
			{
				var i = 1;
				while (!Position.IsOutOfBounds(fileAsIndex + sign1 * i, rankAsIndex))
				{
					Position.AddIfInBounds(fileAsIndex + sign1 * i, rankAsIndex, res);
					i++;
				}
			}

			for (var sign2 = -1; sign2 <= 1; sign2 += 2)
			{
				var i = 1;
				while (!Position.IsOutOfBounds(fileAsIndex, rankAsIndex + sign2 * i))
				{
					res.Add(new Position(fileAsIndex, rankAsIndex + sign2 * i));
					i++;
				}
			}
			
			for (var sign1 = -1; sign1 <= 1; sign1 += 2)
			{
				for (var sign2 = -1; sign2 <= 1; sign2 += 2)
				{
					var i = 1;
					while (!Position.IsOutOfBounds(fileAsIndex+sign1 *i, rankAsIndex+sign2* i))
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
