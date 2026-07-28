using Events;

namespace Chess.Core.Pieces {
	internal class Knight : ChessPiece {
		public Knight(Colour colour) : base(colour) {
		}

		public override string ToFenCharecter() {
			return colour == Colour.White ? "N" : "n";
		}

		public override HashSet<Position> GetPossibleMoves(Position startPosition)
		{
			
			throw new NotImplementedException();
		}
	}
}
