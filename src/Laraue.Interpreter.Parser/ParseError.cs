using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Parser;

/// <summary>
/// One of the parsing errors.
/// </summary>
/// <typeparam name="TTokenType"></typeparam>
public class ParseError<TTokenType> where TTokenType : struct, Enum
{
    /// <summary>
    /// Token related to the error.
    /// </summary>
    public required Token<TTokenType> Token { get; init; }
    
    /// <summary>
    /// Error text.
    /// </summary>
    public required string Error { get; init; }
}