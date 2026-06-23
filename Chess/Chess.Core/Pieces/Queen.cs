namespace Chess.Core.Pieces {
	public class Queen : ChessPiece {
		public Queen(Colour colour) : base(colour) {
		}

		public override string ToFenCharecter() {
			return colour == Colour.White ? "Q" : "q";
		}
	}
}
