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

    /// <summary>Validates all six FEN fields and returns a consistently spaced value.</summary>
    public static bool TryNormalize(string? fen, out string normalized, out string error)
    {
        normalized = string.Empty;
        error = string.Empty;

        var fields = fen?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (fields.Length != 6)
            return Fail("A FEN must contain all six fields.", out error);

        var ranks = fields[0].Split('/');
        if (ranks.Length != 8)
            return Fail("Piece placement must contain eight ranks.", out error);

        var whiteKings = 0;
        var blackKings = 0;
        foreach (var rank in ranks)
        {
            var squares = 0;
            foreach (var symbol in rank)
            {
                if (symbol is >= '1' and <= '8')
                {
                    squares += symbol - '0';
                    continue;
                }

                if (!"prnbqkPRNBQK".Contains(symbol))
                    return Fail($"'{symbol}' is not a valid piece symbol.", out error);

                squares++;
                if (symbol == 'K') whiteKings++;
                if (symbol == 'k') blackKings++;
            }

            if (squares != 8)
                return Fail("Every rank must describe exactly eight squares.", out error);
        }

        if (whiteKings != 1 || blackKings != 1)
            return Fail("The position must contain exactly one king of each colour.", out error);

        if (fields[1] is not ("w" or "b"))
            return Fail("The active colour must be 'w' or 'b'.", out error);

        if (!IsValidCastlingField(fields[2]))
            return Fail("Castling rights must be '-' or a unique combination of KQkq.", out error);

        if (!IsValidEnPassantField(fields[3], fields[1]))
            return Fail("The en-passant target must be '-' or a square on rank 3 or 6 for the active colour.", out error);

        if (!int.TryParse(fields[4], out var halfMoves) || halfMoves < 0)
            return Fail("The halfmove clock must be a non-negative number.", out error);

        if (!int.TryParse(fields[5], out var fullMoves) || fullMoves < 1)
            return Fail("The fullmove number must be at least 1.", out error);

        normalized = string.Join(' ', fields);
        return true;
    }

    private static bool IsValidCastlingField(string value)
    {
        if (value == "-") return true;
        if (value.Length is < 1 or > 4) return false;

        var seen = new HashSet<char>();
        return value.All(c => "KQkq".Contains(c) && seen.Add(c));
    }

    private static bool IsValidEnPassantField(string value, string activeColour)
    {
        if (value == "-") return true;
        if (value.Length != 2 || value[0] is < 'a' or > 'h') return false;
        return activeColour == "w" ? value[1] == '6' : value[1] == '3';
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }

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
