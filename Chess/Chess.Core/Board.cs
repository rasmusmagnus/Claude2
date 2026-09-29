using System.Diagnostics.CodeAnalysis;
using Chess.Core.Pieces;
using Events;
using Events.Commands;
using Events.Events;

namespace Chess.Core;

public class Board
{
    private readonly IMoveValidator _moveValidator;
    private readonly IEventProducer<IGameEvent> _producer;
    private readonly IEventConsumer<ICommand> _consumer;

    public static string StartBoardFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    public Colour MovingColour { get; set; } = Colour.White;

    public CastlingState WhiteCastlingState = new();
    public CastlingState BlackCastlingState = new();

    public Position? EnPassantSquare { get; set; }

    private int _halfMoves = 0;
    private int _FullMoves = 1;
    private int _lastCaptureFullMove = 1;

    public BoardPositions Positions;

    private CountDictionary _stateHistory;

    private class CountDictionary
    {
        private readonly Dictionary<string, int> _dictionary = new();

        public bool Add(string fen)
        {
            if (_dictionary.TryAdd(fen, 1))
                return false;
            var newVal =_dictionary[fen] + 1;
            _dictionary[fen] = newVal;
            if (newVal > 3)
                return true;
            return false;
        }

        public void Clear()
        {
            _dictionary.Clear();
        }
    }

    public Board(IMoveValidator moveValidator, IEventProducer<IGameEvent> producer, IEventConsumer<ICommand> consumer,
        string fenString)
    {
        _moveValidator = moveValidator;
        _producer = producer;
        _consumer = consumer;
        Positions = new BoardPositions(fenString);
        _stateHistory = new ();
        MovingColour = GetTurnFromFen(fenString);
        var castlingStates = GetCaslingStatesFromFen(fenString);
        WhiteCastlingState = castlingStates.white;
        BlackCastlingState = castlingStates.black;
        _stateHistory.Add(ToFen());
    }

    public Board(IMoveValidator moveValidator, IEventProducer<IGameEvent> producer, IEventConsumer<ICommand> consumer)
    {
        _moveValidator = moveValidator;
        _producer = producer;
        _consumer = consumer;
        Positions = new BoardPositions(StartBoardFen);
        _stateHistory = new ();
        MovingColour = GetTurnFromFen(StartBoardFen);
        var castlingStates = GetCaslingStatesFromFen(StartBoardFen);
        WhiteCastlingState = castlingStates.white;
        BlackCastlingState = castlingStates.black;
        _stateHistory.Add(ToFen());
        _producer.SubmitEvent(new NewGameEvent(ToFen()));
    }

    public async Task RunAsync(CancellationToken token)
    {
        var evt = new BoardUpdateEvent(ToFen());

        _producer.SubmitEvent(evt);


        var reader = _consumer.GetReader();

        while (!token.IsCancellationRequested)
        {
            var res = await reader.ReadAsync(token);
            switch (res)
            {
                case MakeMoveCommand moveCommand:
                {
                    HandleMoveCommand(moveCommand);
                    break;
                }
                case NewGameCommand newGameCommand:
                {
                    HandleNewGameCommand(newGameCommand);
                    break;
                }
                case PromoteToCommand promoteToCommand:
                {
                    HandlePromoteToCommand(promoteToCommand);
                    break;
                }
            }
        }
    }

    private void HandlePromoteToCommand(PromoteToCommand promoteToCommand)
    {
        Positions.FinalizePromote(promoteToCommand.Option);
        var fen = ToFen();
        if (_stateHistory.Add(fen))
        {
            _producer.SubmitEvent(new GameEndedEvent(GameResult.Draw));
            return;
        }
        _producer.SubmitEvent(new BoardUpdateEvent(fen));
        if (IsInCheckOrStaleMate(MovingColour, out var colour, out var isMate, out var isStaleMate))
        {
            if(!isStaleMate)
                _producer.SubmitEvent(new CheckEvent(colour!.Value));
            if (isMate)
                _producer.SubmitEvent(
                    new GameEndedEvent(colour == Colour.Black ? GameResult.WhiteWon : GameResult.BlackWon));
            else if (isStaleMate)
            {
                _producer.SubmitEvent(
                    new GameEndedEvent(GameResult.Draw));
            }
        }
    }

    private void HandleNewGameCommand(NewGameCommand newGameCommand)
    {
        ResetState();
    }

    private void ResetState()
    {
        Positions = new BoardPositions(StartBoardFen);
        _stateHistory = new ();
        MovingColour = GetTurnFromFen(StartBoardFen);
        var castlingStates = GetCaslingStatesFromFen(StartBoardFen);
        WhiteCastlingState = castlingStates.white;
        BlackCastlingState = castlingStates.black;
        _stateHistory.Add(ToFen());
        _producer.SubmitEvent(new NewGameEvent(ToFen()));
    }

    private void ProgressTurn(bool moveWasCaptureOrPawnMove)
    {
        if (moveWasCaptureOrPawnMove)
        {
            _halfMoves = 0;
            _lastCaptureFullMove = _FullMoves;
        }
        else
            _halfMoves++;
        if(_halfMoves >= 100 || (_FullMoves - _lastCaptureFullMove >= 50))
            _producer.SubmitEvent(new GameEndedEvent(GameResult.Draw));

        if (MovingColour == Colour.White)
        {
            MovingColour = Colour.Black;
            return;
        }

        _FullMoves++;
        MovingColour = Colour.White;
    }

    public void HandleMoveCommand(MakeMoveCommand command)
    {
        if (!TryGetPieceAtPosition(command.Move.From, out var piece))
            return;

        var isKingSideCastlingMove = false;
        var isQueenSideCastlingMove = false;
        var startPos = piece.Colour == Colour.White ? King.WhiteStartPosition : King.BlackStartPosition;
        if (command.Move.From != startPos)
        {
            isKingSideCastlingMove = false;
            isQueenSideCastlingMove = false;
        }
        else if (command.Move.To.File == startPos.File - 2)
        {
            isQueenSideCastlingMove = true;
        }
        else if (command.Move.To.File == startPos.File + 2)
        {
            isKingSideCastlingMove = true;
        }

        if (!_moveValidator.Validate(command, piece, this, isQueenSideCastlingMove, isKingSideCastlingMove))
            return;

        var isPromotingMove = false;

        if (Positions[command.Move.From] is Pawn pawn && (command.Move.To.Rank == 1 || command.Move.To.Rank == 8))
        {
            _producer.SubmitEvent(new PromotingEvent(pawn.Colour, command.Move.To));
            isPromotingMove = true;
        }

        var isDoublePawnMove = piece is Pawn && (command.Move.From - command.Move.To == (0, 2) ||
                                                 command.Move.From - command.Move.To == (0, -2));

        var isEnPassantCapture = piece is Pawn && command.Move.To == EnPassantSquare;
        MakeMove(command.Move, isQueenSideCastlingMove, isKingSideCastlingMove, isDoublePawnMove, isEnPassantCapture,
            isPromotingMove);
        if (isPromotingMove)
            return;
        _producer.SubmitEvent(new BoardUpdateEvent(ToFen()));

        if (IsInCheckOrStaleMate(MovingColour == Colour.Black ? Colour.White : Colour.Black, out var colour, out var isMate, out var isStaleMate))
        {
            
            if (isMate)
            {
                _producer.SubmitEvent(new CheckEvent(colour!.Value));
                _producer.SubmitEvent(new GameEndedEvent(colour == Colour.Black ? GameResult.WhiteWon : GameResult.BlackWon));
            }
            else if(isStaleMate)
                _producer.SubmitEvent(new GameEndedEvent(GameResult.Draw));
            else
            {
                _producer.SubmitEvent(new CheckEvent(colour!.Value));
            }
        }
    }

    private bool IsInCheckOrStaleMate(Colour movingColour, out Colour? colour, out bool isMate, out bool isStaleMate)
    {
        colour = null;
        var isWhiteInCheck =
            MoveValidator.IsBoardInCheck(Positions.GetKingPosition(Colour.White), Colour.White, Positions);
        var isBlackInCheck =
            MoveValidator.IsBoardInCheck(Positions.GetKingPosition(Colour.Black), Colour.Black, Positions);
        isStaleMate = false;
        if (isWhiteInCheck)
        {
            colour = Colour.White;
            isMate = MoveValidator.IsKingInCheckMate(Positions.GetKingPosition(Colour.White), Colour.White, this);
            return true;
        }
        else if (isBlackInCheck)
        {
            colour = Colour.Black;
            isMate = MoveValidator.IsKingInCheckMate(Positions.GetKingPosition(Colour.Black), Colour.Black, this);
            return true;
        }
        else
        {
            isMate = false;
            var opposite = movingColour == Colour.Black ? Colour.White : Colour.Black;
            isStaleMate = MoveValidator.IsKingInCheckMate(Positions.GetKingPosition(opposite), opposite, this);
            return isStaleMate;
        }

    }

    private Colour GetTurnFromFen(string fenString)
    {
        var turnHolderString = fenString.Split(" ")[1].ToLower();
        return turnHolderString == "w" ? Colour.White : Colour.Black;
    }

    public (CastlingState white, CastlingState black) GetCaslingStatesFromFen(string fen)
    {
        var whiteRes = new CastlingState();
        var blackResult = new CastlingState();

        var castling = fen.Split(" ")[2];

        if (!castling.Contains("Q"))
        {
            whiteRes.RemoveQueensideCastlingRights();
        }

        if (!castling.Contains("K"))
        {
            whiteRes.RemoveKingsideCastlingRights();
        }

        if (!castling.Contains("q"))
        {
            blackResult.RemoveQueensideCastlingRights();
        }

        if (!castling.Contains("k"))
        {
            blackResult.RemoveKingsideCastlingRights();
        }

        return (whiteRes, blackResult);
    }

    private Board(string producer)
    {
        throw new NotImplementedException();
    }

    public Board GetBoardFromFen(string fen)
    {
        return new Board(fen);
    }

    public void MakeMove(IChessMove move, bool isQueenSideCastlingMove, bool isKingSideCastlingMove,
        bool isDoublePawnMove, bool isEnPassantCapture, bool isPromotingMove)
    {
        var wasCaptureMove = false;
        if (isPromotingMove)
        {
            Positions.PreparePromote(move);
            EnPassantSquare = null;
        }
        else if (isQueenSideCastlingMove || isKingSideCastlingMove)
        {
            Positions.MovePiecesForCastling(move, isQueenSideCastlingMove, isKingSideCastlingMove);
            EnPassantSquare = null;
        }
        else if (isDoublePawnMove)
        {
            var from = move.From;
            var to = move.To;
            var dir = to - from;
            var unitDir = (0, dir.Item2 / Math.Abs(dir.Item2));
            EnPassantSquare = from + unitDir;
            wasCaptureMove = Positions.MovePieces(move);
        }
        else if (isEnPassantCapture)
        {
            wasCaptureMove = Positions.MovePiecesUnderEnPassantAttack(move);
            EnPassantSquare = null;
        }
        else
        {
            wasCaptureMove = Positions.MovePieces(move);
            EnPassantSquare = null;
        }

        ProgressTurn(wasCaptureMove);
        if (TryGetPieceAtPosition(move.To, out var piece))
        {
            if (piece is King)
            {
                var state = piece.Colour == Colour.White ? WhiteCastlingState : BlackCastlingState;
                state.RemoveKingsideCastlingRights();
                state.RemoveQueensideCastlingRights();
            }

            if (piece is Rook rook)
            {
                if (rook.Colour == Colour.White)
                {
                    if (move.From == new Position('a', 1))
                        WhiteCastlingState.RemoveQueensideCastlingRights();
                    else if (move.From == new Position('h', 1))
                        WhiteCastlingState.RemoveKingsideCastlingRights();
                }

                if (rook.Colour == Colour.Black)
                {
                    if (move.From == new Position('a', 8))
                        BlackCastlingState.RemoveQueensideCastlingRights();
                    else if (move.From == new Position('h', 8))
                        BlackCastlingState.RemoveKingsideCastlingRights();
                }
            }
        }

        if (!isPromotingMove)
        {
            if (_stateHistory.Add(ToFen()))
            {
                _producer.SubmitEvent(new GameEndedEvent(GameResult.Draw));
                return;
            }
        }
    }

    public string ToFen()
    {
        var res = Positions.GetPiecesFenPart();
        res += " ";
        res += MovingColour == Colour.White ? "w" : "b";
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
        return EnPassantSquare == null ? "-" : EnPassantSquare.Value.ToString();
    }

    private string FenFromCastlingStates()
    {
        var res = "";

        if (WhiteCastlingState.KingsideAvailable)
        {
            res += "K";
        }

        if (WhiteCastlingState.QueenSideAvailable)
        {
            res += "Q";
        }

        if (BlackCastlingState.KingsideAvailable)
        {
            res += "k";
        }

        if (BlackCastlingState.QueenSideAvailable)
        {
            res += "q";
        }

        if (res == "")
        {
            res += "-";
        }

        return res;
    }


    public bool TryGetPieceAtPosition(Position position, [NotNullWhen(true)] out ChessPiece? piece)
    {
        piece = Positions[position];
        return piece is not null;
    }

    public bool IsPieceBetween(IChessMove commandMove)
    {
        var from = commandMove.From;
        var to = commandMove.To;
        var direction = to - from;
        var unitDir = (direction.Item1 / (direction.Item1 == 0 ? 1 : Math.Abs(direction.Item1)),
            direction.Item2 / (direction.Item2 == 0 ? 1 : Math.Abs(direction.Item2)));
        var resultingPos = from + unitDir;
        while (!Position.IsOutOfBounds(resultingPos) && resultingPos != to)
        {
            var pieceAtPos = Positions[resultingPos];
            if (pieceAtPos is { } piece)
            {
                return true;
            }

            resultingPos = resultingPos + unitDir;
        }

        return false;
    }

    public Position? GetEnPassantSquare()
    {
        return EnPassantSquare;
    }
}

public class CastlingState
{
    private static readonly Position[] KingWhiteSideCastlingSquares = [new Position('f', 1), new Position('g', 1)];

    private static readonly Position[] QueenWhiteSideCastlingSquares =
        [new Position('b', 1), new Position('c', 1), new Position('d', 1)];

    private static readonly Position[] KingBlackSideCastlingSquares = [new Position('f', 8), new Position('g', 8)];

    private static readonly Position[] QueenBlackSideCastlingSquares =
        [new Position('b', 8), new Position('c', 8), new Position('d', 8)];

    public static Position[] KingSideCastlingSquares(Colour colour)
    {
        return colour == Colour.White ? KingWhiteSideCastlingSquares : KingBlackSideCastlingSquares;
    }

    public static Position[] QueenSideCastlingSquares(Colour colour)
    {
        return colour == Colour.White ? QueenWhiteSideCastlingSquares : QueenBlackSideCastlingSquares;
    }

    public bool KingsideAvailable { get; private set; } = true;
    public bool QueenSideAvailable { get; private set; } = true;

    public void RemoveKingsideCastlingRights()
    {
        KingsideAvailable = false;
    }

    public void RemoveQueensideCastlingRights()
    {
        QueenSideAvailable = false;
    }
}