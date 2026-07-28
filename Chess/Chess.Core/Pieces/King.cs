using Events;

namespace Chess.Core.Pieces {
	public class King : ChessPiece {
		public King(Colour colour) : base(colour) {
		}

		public override string ToFenCharecter() {
			return colour == Colour.White ? "K" : "k";
		}

		public override HashSet<Position> GetPossibleMoves(Position startPosition)
		{
			var res = new HashSet<Position>();
			var fileAsIndex = startPosition.FileToIndex();
			var rankAsIndex = startPosition.Rank;
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if(i == 0 && j == 0)
						continue;
					var resultingRank = rankAsIndex + j;
					var resultingFile = fileAsIndex + i;
					
					if(Position.IsOutOfBounds(resultingRank, resultingFile))
						continue;
					
					res.Add(new Position(resultingFile, resultingRank));
				}
			}
			return res;
		}
		
	}
}
