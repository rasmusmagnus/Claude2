using Events;

namespace Chess.Core.Pieces {
	public abstract class ChessPiece {
		public readonly Colour Colour;

		public ChessPiece(Colour colour) {
			this.Colour = colour;
		}

		public abstract string ToFenCharecter();

		public abstract HashSet<Position> GetPossibleMoves(Position startPosition);
	}
}
