namespace Chess.Core.Tests;

public class BoardStateTests {
	[Theory]
	[InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")]
	[InlineData("8/8/8/8/8/8/8/8 w KQkq - 0 1")]
	[InlineData("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1")]
	[InlineData("rnbqkbnr/pp1ppppp/8/2p5/4P3/8/PPPP1PPP/RNBQKBNR w KQkq c6 0 2")]
	[InlineData("4P3/8/8/8/8/8/8/8 b KQkq e3 0 1")]
	public void CheckEasyFen(string fen) {
		var board = new BoardState(fen);
		Assert.Equal(fen.Split(" ")[0], board.GetPiecesFenPart());
	}
}
