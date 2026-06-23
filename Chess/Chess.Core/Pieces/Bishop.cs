namespace Chess.Core.Pieces {
	internal class Bishop : ChessPiece {
		public Bishop(Colour colour) : base(colour) {
		}

		public override string ToFenCharecter() {
			return colour == Colour.White ? "B" : "b";
		}
	}
}
