namespace Chess.Core.Pieces {
	internal class King : ChessPiece {
		public King(Colour colour) : base(colour) {
		}

		public override string ToFenCharecter() {
			return colour == Colour.White ? "K" : "k";
		}
	}
}
