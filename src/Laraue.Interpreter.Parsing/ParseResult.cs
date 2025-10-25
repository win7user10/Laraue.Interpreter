namespace Laraue.Interpreter.Parsing;

/// <summary>
/// The result of token parse.
/// </summary>
/// <typeparam name="TTokenType"></typeparam>
/// <typeparam name="TResult"></typeparam>
public class ParseResult<TTokenType, TResult>
    where TTokenType : struct, Enum
    where TResult : class
{
    /// <summary>
    /// Parsed AST. Null when errors occurred.
    /// </summary>
    public TResult? Result { get; set; }
    
    /// <summary>
    /// The occurred errors list.
    /// </summary>
    public required ParseError<TTokenType>[] Errors { get; set; }
    
    /// <summary>
    /// Did any error occur?
    /// </summary>
    public bool HasErrors => Errors.Length != 0;
}