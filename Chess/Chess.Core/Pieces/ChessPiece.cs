namespace Chess.Core.Pieces {
	public abstract class ChessPiece {
		public readonly Colour colour;

		public ChessPiece(Colour colour) {
			this.colour = colour;
		}

		public abstract string ToFenCharecter();
	}
}
