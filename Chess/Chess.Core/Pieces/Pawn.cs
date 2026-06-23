namespace Chess.Core.Pieces {
	public class Pawn : ChessPiece {
		public Pawn(Colour colour) : base(colour) {
		}

		public override string ToFenCharecter() {
			return colour == Colour.White ? "P" : "p";
		}
	}
}
