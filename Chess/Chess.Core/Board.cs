using Events;
using Events.Commands;
using Events.Events;

namespace Chess.Core;

public class Board {
	private readonly IMoveValidator _moveValidator;
	private readonly IEventProducer<IGameEvent> _producer;
	private readonly IEventConsumer<ICommand> _consumer;

	public static string StartBoardFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

	public bool WhiteHasMove;

	public CastlingState whiteCastlingState;
	public CastlingState blackCastlingState;


	public BoardPositions Positions;

	public List<string> stateHistory;

	public Board(IEventProducer<IGameEvent> producer, IEventConsumer<IGameEvent> consumer) {
		_producer = producer;
		_consumer = consumer;
		Positions = new BoardPositions(StartBoardFen);
		stateHistory = new List<string>();
		stateHistory.Add(ToFen());
		WhiteHasMove = GetTurnFromFen(StartBoardFen);
		var castlingStates = GetCaslingStatesFromFen(StartBoardFen);
		whiteCastlingState = castlingStates.white;
		blackCastlingState = castlingStates.black;

	}

	public bool GetTurnFromFen(string fenString) {
		var turnHolderString = fenString.Split(" ")[1].ToLower();
		return turnHolderString == "w";
	}

	public (CastlingState white, CastlingState black) GetCaslingStatesFromFen(string fen) {
		var whiteRes = new CastlingState();
		var blackResult = new CastlingState();


		var castling = fen.Split(" ")[2].ToLower();

		if (!castling.Contains("Q")) {
			whiteRes.RemoveQueensideCastlingRights();
		}
		if (!castling.Contains("K")) {
			whiteRes.RemoveKingsideCastlingRights();
		}
		if (!castling.Contains("q")) {
			blackResult.RemoveQueensideCastlingRights();
		}
		if (!castling.Contains("k")) {
			blackResult.RemoveKingsideCastlingRights();
		}


		return (whiteRes, blackResult);

	}

	private Board(string producer) {
		throw new NotImplementedException();
	}

	public Board GetBoardFromFen(string fen) {


		return new Board(fen);
	}

	public void MakeMove(IChessMove move) {
		throw new NotImplementedException();

		stateHistory.Add(ToFen());
	}

	public string ToFen() {

		var res = Positions.GetPiecesFenPart();
		res += " ";
		res += WhiteHasMove ? "w" : "b";
		res += " ";
		res += FenFromCastlingStates();


		return res;
	}

	private string FenFromCastlingStates()
	{
		var res = "";

		if (whiteCastlingState.KingsideAvailable)
		{
			res += "K";
		}

		if (whiteCastlingState.QueenSideAvailable)
		{
			res += "Q";
		}

		if (blackCastlingState.KingsideAvailable)
		{
			res += "k";
		}

		if (blackCastlingState.QueenSideAvailable)
		{
			res += "q";
		}

		if (res == "")
		{
			res += "-";
		}

		return res;
	}

	public async Task RunAsync(CancellationToken token) {
		var evt = new BoardUpdateEvent();

		_producer.SubmitEvent(evt);

		var reader = _consumer.GetReader();

		while (!token.IsCancellationRequested) {
			var res = await reader.ReadAsync(token);
			switch (res) {
				case BoardUpdateEvent boardUpdateEvent: {
						HandleBoardUpdateEvent(boardUpdateEvent);
						break;
					}
			}
		}
	}

	private void HandleBoardUpdateEvent(BoardUpdateEvent boardUpdateEvent) {
		throw new NotImplementedException();
	}
}

public class CastlingState {
	public bool KingsideAvailable { get; private set; } = true;
	public bool QueenSideAvailable { get; private set; } = true;

	public void RemoveKingsideCastlingRights() {
		KingsideAvailable = false;
	}

	public void RemoveQueensideCastlingRights() {
		QueenSideAvailable = false;
	}


}

