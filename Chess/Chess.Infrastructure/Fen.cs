namespace Events;

/// <summary>
/// A read-only helper for the UI: it expands the piece-placement portion of a
/// FEN string into a grid the board can render. It contains NO chess logic and,
/// deliberately, no way to <em>mutate</em> a position — producing the next
/// position from a move belongs to the core domain, which emits the resulting
/// FEN back to the UI via <c>BoardUpdateEvent</c>.
/// </summary>
public static class Fen
{
    /// <summary>The standard chess starting position.</summary>
    public const string StartPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    /// <summary>
    /// Expands the placement field into a rank-major grid where
    /// <c>grid[rank]</c> is rank 1..8 (index 0 = rank 1) and each entry is
    /// file a..h. Empty squares are <c>'\0'</c>.
    /// </summary>
    public static char[][] ToGrid(string fen)
    {
        var placement = fen.Split(' ')[0];
        var rows = placement.Split('/');
        var grid = new char[8][];

        for (var i = 0; i < rows.Length; i++)
        {
            // FEN lists rank 8 first, so row i maps to rank (8 - i) => index 7 - i.
            var rank = new char[8];
            var file = 0;
            foreach (var c in rows[i])
            {
                if (char.IsDigit(c))
                {
                    var empty = c - '0';
                    for (var k = 0; k < empty; k++) rank[file++] = '\0';
                }
                else
                {
                    rank[file++] = c;
                }
            }

            grid[7 - i] = rank;
        }

        return grid;
    }
}
