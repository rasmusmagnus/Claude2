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

	private CastlingState _whiteCastlingState = new();
	private CastlingState _blackCastlingState = new();

	private int _halfMoves = 0;
	private int _FullMoves = 1;
	
	public BoardPositions Positions;

	public List<string> stateHistory;

	public Board(IMoveValidator moveValidator, IEventProducer<IGameEvent> producer, IEventConsumer<ICommand> consumer) {
		_moveValidator = moveValidator;
		_producer = producer;
		_consumer = consumer;
		Positions = new BoardPositions(StartBoardFen);
		stateHistory = new List<string>();
		WhiteHasMove = GetTurnFromFen(StartBoardFen);
		var castlingStates = GetCaslingStatesFromFen(StartBoardFen);
		_whiteCastlingState = castlingStates.white;
		_blackCastlingState = castlingStates.black;
		stateHistory.Add(ToFen());
	}
	public async Task RunAsync(CancellationToken token) {
		var evt = new BoardUpdateEvent(ToFen());

		_producer.SubmitEvent(evt);

		var reader = _consumer.GetReader();

		while (!token.IsCancellationRequested) {
			var res = await reader.ReadAsync(token);
			switch (res) {
				case MakeMoveCommand moveCommand:
				{
					HandleMoveCommand(moveCommand);
					break;
				}
			}
		}
	}

	private void HandleMoveCommand(MakeMoveCommand moveCommand)
	{
		if (!_moveValidator.Validate(moveCommand))
			return;
		
		MakeMove(moveCommand.Move);
		_producer.SubmitEvent(new BoardUpdateEvent(ToFen()));
	}


	public bool GetTurnFromFen(string fenString) {
		var turnHolderString = fenString.Split(" ")[1].ToLower();
		return turnHolderString == "w";
	}

	public (CastlingState white, CastlingState black) GetCaslingStatesFromFen(string fen) {
		var whiteRes = new CastlingState();
		var blackResult = new CastlingState();

		var castling = fen.Split(" ")[2];

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

	public Board GetBoardFromFen(string fen) 
	{
		return new Board(fen);
	}

	public void MakeMove(IChessMove move)
	{
		Positions.MovePieces(move.From, move.To);
		stateHistory.Add(ToFen());
	}

	public string ToFen() 
	{
		var res = Positions.GetPiecesFenPart();
		res += " ";
		res += WhiteHasMove ? "w" : "b";
		res += " ";
		res += FenFromCastlingStates();
		res += " ";
		res += GetEnPassantTiles();
		res += " ";
		res += _halfMoves;
		res += " ";
		res += _FullMoves;
		return res;
	}

	private string GetEnPassantTiles()
	{
		return "-";
	}

	private string FenFromCastlingStates()
	{
		var res = "";

		if (_whiteCastlingState.KingsideAvailable)
		{
			res += "K";
		}

		if (_whiteCastlingState.QueenSideAvailable)
		{
			res += "Q";
		}

		if (_blackCastlingState.KingsideAvailable)
		{
			res += "k";
		}

		if (_blackCastlingState.QueenSideAvailable)
		{
			res += "q";
		}

		if (res == "")
		{
			res += "-";
		}

		return res;
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

