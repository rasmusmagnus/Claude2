using System.Diagnostics;
using Chess.Core.Pieces;
using Events;

namespace Chess.Core {
	public class BoardPositions {
		public ChessPiece?[][] state;
		
		public ChessPiece? this[Position position] {
			get
			{
				var (indexFinal, letterIndex) = GetIndexFromNotation(position);

				return state[indexFinal][letterIndex];
			}
			private set
			{
				var (indexFinal, letterIndex) = GetIndexFromNotation(position);
				state[indexFinal][letterIndex] = value;
			}
		}

		private static (int, int) GetIndexFromNotation(Position position)
		{
			int indexInt = position.File switch
			{
				'a' => 0,
				'b' => 1,
				'c' => 2,
				'd' => 3,
				'e' => 4,
				'f' => 5,
				'g' => 6,
				'h' => 7,
				_ => throw new ArgumentException("Invalid index")
			};
			return (position.Rank - 1, indexInt);
		}

		public BoardPositions(string fenString) {
			var result = new ChessPiece?[8][];
			var trimmedFen = fenString.Split(" ")[0];

			var fenRows = trimmedFen.Split("/");

			for (int i = 0;
			i < fenRows.Length; i++) {
				var file = fenRows[i];
				result[7 - i] = new ChessPiece?[8];
				var squareIndex = 0;
				for (int j = 0; j < file.Length; j++) {
					var square = file[j];


					if (int.TryParse(square.ToString(), out var count)) {
						for (int k = 0; k < count; k++) {
							result[7 - i][squareIndex] = null;
							squareIndex++;
						}
					} else {
						var colour = square.ToString().ToLower().Equals(square.ToString()) ? Colour.Black : Colour.White;
						ChessPiece piece;
						switch (square.ToString().ToLower()) {
							case "r":
								piece = new Rook(colour); break;
							case "p":
								piece = new Pawn(colour); break;
							case "b":
								piece = new Bishop(colour); break;
							case "n":
								piece = new Knight(colour); break;
							case "k":
								piece = new King(colour); break;
							case "q":
								piece = new Queen(colour); break;
							default:
								throw new Exception("wfts");
						}

						result[7 - i][squareIndex] = piece;
						squareIndex++;
					}

				}

			}
			state = result;
		}

		public string GetPiecesFenPart() {
			var result = "";
			foreach (var file in state.Reverse()) {
				var counter = 0;
				foreach (var square in file) {

					if (square != null) {
						if (counter != 0) {
							result += counter;
							counter = 0;
						}

						result += square.ToFenCharecter();
					} else {
						counter++;
					}


				}
				if (counter != 0) {
					result += counter;
				}

				result += "/";


			}

			return result[..^1];
		}

		public void MovePieces(Position from, Position to)
		{
			var fromPiece = this[from];
			Debug.Assert(fromPiece != null);
			this[from] = null;
			this[to] = fromPiece;
			Debug.Assert(this[from] == null);
		}
	}
}
