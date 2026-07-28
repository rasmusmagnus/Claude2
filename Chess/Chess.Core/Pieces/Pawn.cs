using Events;

namespace Chess.Core.Pieces {
	public class Pawn : ChessPiece {
		public Pawn(Colour colour) : base(colour) {
		}

		public override string ToFenCharecter() {
			return colour == Colour.White ? "P" : "p";
		}

		public override HashSet<Position> GetPossibleMoves(Position startPosition)
		{
			var res = new HashSet<Position>();
			var fileAsIndex = startPosition.FileToIndex();
			var rankAsIndex = startPosition.Rank;
			var moveDirectionSign = colour == Colour.White ? 1 : -1;
			var startRank = colour == Colour.White ? 2 : 7;
			if (rankAsIndex == startRank)
			{
				Position.AddIfInBounds(rankAsIndex + moveDirectionSign * 2, fileAsIndex , res);
			}

			for (var i = -1; i <= 1; i++)
			{
				Position.AddIfInBounds(rankAsIndex + moveDirectionSign, fileAsIndex + i, res);
			}
			return res;
		}
		
	}
}
