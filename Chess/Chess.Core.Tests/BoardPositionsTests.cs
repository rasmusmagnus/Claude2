using Chess.Core.Pieces;

namespace Chess.Core.Tests;

public class BoardPositionsTests {
	[Theory]
	[InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
	[InlineData("8/8/8/8/8/8/8/8 w KQkq - 0 1")]
	[InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1")]
	[InlineData("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2")]
	[InlineData("4P3/8/8/8/8/8/8/8 b KQkq e3 0 1")]
	public void CheckEasyFen(string fen) {
		var board = new BoardPositions(fen);
		Assert.Equal(fen.Split(" ")[0], board.GetPiecesFenPart());
	}
	
	[Fact]
	public void ByPosition() {
		var fen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
		var board = new BoardPositions(fen);

		var piece = board["e", 4];
		Assert.True(piece is Pawn);
		Assert.Equal(Colour.White, piece.colour);
	}

	[Fact]
	public void ByPosition2() {
		var fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
		var board = new BoardPositions(fen);

		var piece1 = board["d", 1];
		Assert.Equal(typeof(Queen), piece1?.GetType());
		Assert.True(piece1 is Queen);
		Assert.Equal(Colour.White, piece1.colour);

		var piece2 = board["d", 8];
		Assert.True(piece2 is Queen);
		Assert.Equal(Colour.Black, piece2.colour);

	}
}
