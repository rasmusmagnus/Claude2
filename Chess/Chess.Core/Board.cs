using Chess.Core.Pieces;
using Events;
using Events.Commands;
using Events.Events;

namespace Chess.Core;

public class Board {
	private readonly IMoveValidator _moveValidator;
	private readonly IEventProducer<IGameEvent> _producer;
	private readonly IEventConsumer<ICommand> _consumer;

	public static string StartBoardFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
	
	public BoardState customState;

	public Board(IMoveValidator moveValidator, IEventProducer<IGameEvent> producer, IEventConsumer<ICommand> consumer) {
		_moveValidator = moveValidator;
		_producer = producer;
		_consumer = consumer;
	}

	private Board(string producer) {
		throw new NotImplementedException();
	}

	public Board GetBoardFromFen(string fen) {


		return new Board(fen);
	}

	public void MakeMove(IChessMove move) {
	}

	public string ToFen(Board board) {
		return customState.GetPiecesFenPart();
	}

	public async Task RunAsync(CancellationToken token) {
		var evt = new BoardUpdateEvent();

		_producer.SubmitEvent(evt);

		var reader = _consumer.GetReader();

		while (!token.IsCancellationRequested) {
			var res = await reader.ReadAsync(token);
			switch (res) {
				case MakeMoveCommand moveCommand:
				{
					HandleMakeMove(moveCommand);
					break;
				}
			}
		}
	}

	private void HandleMakeMove(MakeMoveCommand moveCommand)
	{
							
	}

	private void HandleBoardUpdateEvent(BoardUpdateEvent boardUpdateEvent) {
		throw new NotImplementedException();
	}
}

public class BoardState {
	public ChessPiece?[][] state;

	public ChessPiece? this[string index, int index2] {
		get {
			int indexInt = index.ToLower() switch
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

			return state[index2 - 1][indexInt];
		}
	}

	public BoardState(string fenString) {
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
}