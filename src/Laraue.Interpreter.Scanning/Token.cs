namespace Laraue.Interpreter.Scanning;

/// <summary>
/// One of the scanned tokens.
/// </summary>
/// <typeparam name="TTokenType"></typeparam>
public class Token<TTokenType> where TTokenType : struct, Enum
{
    private string? _lexemeCache;

    /// <summary>
    /// Token type, e.g., String, number, Comma, etc.
    /// The null token means it is the last token in scan.
    /// </summary>
    public required TTokenType? TokenType { get; init; } //

    /// <summary>
    /// The token lexeme as a zero-copy view into the scanned input. Prefer this over
    /// <see cref="Lexeme"/> when only character-level access is needed, to avoid materializing a string.
    /// </summary>
    public required ReadOnlyMemory<char> LexemeMemory { get; init; }

    /// <summary>
    /// The token lexeme, e.g., "," for Comma, "." for Dot. Computed from <see cref="LexemeMemory"/>
    /// on first access and cached.
    /// </summary>
    public string? Lexeme => _lexemeCache ??= LexemeMemory.Length == 0 ? null : LexemeMemory.ToString();

    /// <summary>
    /// The token literal, e.g., "Dog" (string) for String token, 5 (int) for Integer token
    /// </summary>
    public object? Literal { get; init; }
    
    /// <summary>
    /// The line number the token from.
    /// </summary>
    public required int LineNumber { get; init; }
    
    /// <summary>
    /// The line position where the token starts.
    /// </summary>
    public required int StartPosition { get; init; }
    
    /// <summary>
    /// The line position where the token finish.
    /// </summary>
    public required int EndPosition { get; init; }
}