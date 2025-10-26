using Laraue.Interpreter.Common;
using Laraue.Interpreter.Common.Extensions;

namespace Laraue.Interpreter.Parsing.Extensions;

public static class ParseResultExtensions
{
    /// <summary>
    /// Get errors from the parse result as <see cref="CompileError"/> list.
    /// </summary>
    public static ICollection<CompileError> GetCompileErrors<TTokenType, TResult>(
        this ParseResult<TTokenType, TResult> parseResult)
        where TTokenType : struct, Enum
        where TResult : class
    {
        var errors = new List<CompileError>();
        
        foreach (var parseError in parseResult.Errors)
        {
            var token = parseError.Token.Lexeme;
            
            errors.Add(new CompileError
            {
                Message = token is null
                    ? $"Syntax error: {parseError.Error}"
                    : $"Syntax error on token '{parseError.Token.Lexeme}': {parseError.Error}",
                StartPosition = parseError.Token.StartPosition,
                EndPosition = parseError.Token.EndPosition,
                StartLineNumber = parseError.Token.LineNumber,
                EndLineNumber = parseError.Token.LineNumber
            });
        }
        
        return errors;
    }
    
    /// <summary>
    /// Throw if any error exists in the parse result,
    /// </summary>
    public static void ThrowOnAnyError<TTokenType, TResult>(
        this ParseResult<TTokenType, TResult> parseResult)
        where TTokenType : struct, Enum
        where TResult : class
    {
        parseResult.GetCompileErrors().ThrowOnAny();
    }
}