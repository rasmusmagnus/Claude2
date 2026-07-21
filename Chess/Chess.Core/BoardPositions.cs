using System.Diagnostics;
using Chess.Core.Pieces;

namespace Chess.Core {
	public class BoardPositions {
		public ChessPiece?[][] state;
		
		public ChessPiece? this[string index, int index2] {
			get
			{
				var (indexFinal, letterIndex) = GetIndexFromNotation(index2, index);

				return state[indexFinal][letterIndex];
			}
			private set
			{
				var (indexFinal, letterIndex) = GetIndexFromNotation(index2, index);
				state[indexFinal][letterIndex] = value;
			}
		}

		private static (int, int) GetIndexFromNotation(int index, string letterIndex)
		{
			int indexInt = letterIndex.ToLower() switch
			{
				"a" => 0,
				"b" => 1,
				"c" => 2,
				"d" => 3,
				"e" => 4,
				"f" => 5,
				"g" => 6,
				"h" => 7,
				_ => throw new ArgumentException("Invalid index")
			};
			return (index - 1, indexInt);
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

		public void MovePieces(string from, string to)
		{
			var fromPiece = this[from[0].ToString(), int.Parse(from[1].ToString())];
			Debug.Assert(fromPiece != null);
			this[from[0].ToString(), int.Parse(from[1].ToString())] = null;
			this[to[0].ToString(), int.Parse(to[1].ToString())] = fromPiece;
			Debug.Assert(this[from[0].ToString(), int.Parse(from[1].ToString())] == null);
		}
	}
}
