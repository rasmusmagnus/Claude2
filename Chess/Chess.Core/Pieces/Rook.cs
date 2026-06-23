using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Core.Pieces {
	internal class Rook : ChessPiece {
		public Rook(Colour colour) : base(colour)
		{
		}

		public override string ToFenCharecter()
		{
			return colour == Colour.White ? "R" : "r";
		}
	}
}
