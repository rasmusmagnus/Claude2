namespace Chess.Core.Pieces {
	internal abstract class ChessPiece {
		protected readonly Colour colour;

		protected ChessPiece(Colour colour) {
			this.colour = colour;
		}

		public abstract string ToFenCharecter();
	}
}
