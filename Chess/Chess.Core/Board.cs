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

    public BoardPositions Positions;

    public List<string> stateHistory;

    public Board(IMoveValidator moveValidator, IEventProducer<IGameEvent> producer, IEventConsumer<ICommand> consumer,
        string fenString)
    {
        _moveValidator = moveValidator;
        _producer = producer;
        _consumer = consumer;
        Positions = new BoardPositions(fenString);
        stateHistory = new List<string>();
        MovingColour = GetTurnFromFen(fenString);
        var castlingStates = GetCaslingStatesFromFen(fenString);
        WhiteCastlingState = castlingStates.white;
        BlackCastlingState = castlingStates.black;
        stateHistory.Add(ToFen());
    }

    public Board(IMoveValidator moveValidator, IEventProducer<IGameEvent> producer, IEventConsumer<ICommand> consumer)
    {
        _moveValidator = moveValidator;
        _producer = producer;
        _consumer = consumer;
        Positions = new BoardPositions(StartBoardFen);
        stateHistory = new List<string>();
        MovingColour = GetTurnFromFen(StartBoardFen);
        var castlingStates = GetCaslingStatesFromFen(StartBoardFen);
        WhiteCastlingState = castlingStates.white;
        BlackCastlingState = castlingStates.black;
        stateHistory.Add(ToFen());
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
            }
        }
    }

    private void ProgressTurn()
    {
        if (MovingColour == Colour.White)
        {
            MovingColour = Colour.Black;
            return;
        }

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

        var isDoublePawnMove = piece is Pawn && (command.Move.From - command.Move.To == (0, 2) ||
                                                 command.Move.From - command.Move.To == (0, -2));

        var isEnPassantCapture = piece is Pawn && command.Move.To == EnPassantSquare;
        MakeMove(command.Move, isQueenSideCastlingMove, isKingSideCastlingMove, isDoublePawnMove, isEnPassantCapture);
        _producer.SubmitEvent(new BoardUpdateEvent(ToFen()));
        if (IsInCheck(out var colour, out var isMate))
            _producer.SubmitEvent(new CheckEvent(colour!.Value, isMate));
    }

    private bool IsInCheck(out Colour? colour, out bool isMate)
    {
        colour = null;
        var isWhiteInCheck =
            MoveValidator.IsBoardInCheck(Positions.GetKingPosition(Colour.White), Colour.White, Positions);
        var isBlackInCheck =
            MoveValidator.IsBoardInCheck(Positions.GetKingPosition(Colour.Black), Colour.Black, Positions);
        
        if(isWhiteInCheck)
        {
            colour = Colour.White;
            isMate = MoveValidator.IsKingInCheckMate(Positions.GetKingPosition(Colour.White), Colour.White, this);
            return true;
        }
        else if (isBlackInCheck)
        {
            isMate = MoveValidator.IsKingInCheckMate(Positions.GetKingPosition(Colour.Black), Colour.Black, this);            
            return true;
        }
        isMate = false;
        return false;
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
        bool isDoublePawnMove, bool isEnPassantCapture)
    {
        if (isQueenSideCastlingMove || isKingSideCastlingMove)
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
            Positions.MovePieces(move);
        }
        else if (isEnPassantCapture)
        {
            Positions.MovePiecesUnderEnPassantAttack(move);
            EnPassantSquare = null;
        }
        else
        {
            Positions.MovePieces(move);
            EnPassantSquare = null;
        }


        ProgressTurn();
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

        stateHistory.Add(ToFen());
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