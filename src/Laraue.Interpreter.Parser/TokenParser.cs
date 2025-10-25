using System.Linq.Expressions;
using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Parser;

/// <summary>
/// Base class to implement a <see href="https://craftinginterpreters.com/parsing-expressions.html">Parser</see>.
/// </summary>
/// <param name="tokens">The token sequence to parse.</param>
/// <typeparam name="TTokenType">Type of tokens to parse.</typeparam>
/// <typeparam name="TParsedExpression">Type of the output expression, something like <see cref="Expression"/> in csharp.</typeparam>
public abstract class TokenParser<TTokenType, TParsedExpression>(Token<TTokenType>[] tokens)
    where TTokenType : struct, Enum
    where TParsedExpression : class
{
    private readonly List<ParseError<TTokenType>> _errors = [];
    private int _current;
    
    /// <summary>
    /// Run the tokens parsing.
    /// </summary>
    /// <returns></returns>
    public ParseResult<TTokenType, TParsedExpression> Parse()
    {
        try
        {
            var result = ParseInternal();
            return new ParseResult<TTokenType, TParsedExpression>
            {
                Result = result,
                Errors = _errors.ToArray()
            };
        }
        catch (ParseException)
        {
            return new ParseResult<TTokenType, TParsedExpression>
            {
                Errors = _errors.ToArray(),
                Result = null
            };
        }
    }

    /// <summary>
    /// The main method to implement the parser.
    /// </summary>
    /// <returns>The parsed AST for the token sequence.</returns>
    protected abstract TParsedExpression ParseInternal();

    /// <summary>
    /// Skip the tokens until the token differs from the passed token types will receive.
    /// </summary>
    protected void Skip(params TTokenType?[] tokenTypes)
    {
        do
        {
        } while (Match(tokenTypes));
    }
    
    /// <summary>
    /// If the next token is match to the passed, go to the next token.
    /// </summary>
    /// <param name="tokenTypes"></param>
    /// <returns></returns>
    protected bool Match(params TTokenType?[] tokenTypes)
    {
        if (!tokenTypes.Any(Check)) 
            return false;
        
        Advance();
        return true;
    }

    /// <summary>
    /// Consume next token of the specified type or throws the <see cref="ParseException"/>.
    /// </summary>
    /// <param name="tokenType">Excepted token.</param>
    /// <param name="message">The exception message.</param>
    /// <exception cref="ParseException"></exception>
    protected Token<TTokenType> Consume(TTokenType tokenType, string message)
    {
        if (Check(tokenType))
            return Advance();
        
        throw Error(Peek(), message);
    }

    /// <summary>
    /// Returns true if the token is equal to the passed.
    /// </summary>
    protected bool Check(TTokenType? exceptedTokenType)
    {
        if (IsParseCompleted)
        {
            return false;
        }

        return EqualityComparer<TTokenType?>.Default.Equals(Peek().TokenType, exceptedTokenType);
    }

    /// <summary>
    /// Returns true if the token with specified offset is equal to the passed.
    /// </summary>
    protected bool Check(int offset, TTokenType? exceptedTokenType)
    {
        if (tokens.Length <= _current + offset)
        {
            return false;
        }

        var realTokenType = tokens[_current + offset].TokenType;
        return EqualityComparer<TTokenType?>.Default.Equals(realTokenType, exceptedTokenType);
    }

    /// <summary>
    /// Returns next offset for the passed token type. 
    /// </summary>
    /// <param name="tokenType"></param>
    /// <returns></returns>
    protected int? GetNextOffset(TTokenType tokenType)
    {
        for (var i = 0; i < tokens.Length - _current; i++)
        {
            if (Check(i, tokenType))
            {
                return i;
            }
        }
        
        return null;
    }

    /// <summary>
    /// Skip tokens that passed in <paramref name="allowsToSkip"/>.
    /// Then check is the next token equal to the passed <see cref="tokenType"/>.
    /// </summary>
    protected bool CheckSkipping(TTokenType tokenType, params TTokenType?[] allowsToSkip)
    {
        for (var i = 0; i < tokens.Length - _current; i++)
        {
            if (Check(i, tokenType))
            {
                return true;
            }

            if (allowsToSkip.Any(x => Check(i, x)))
            {
                continue;
            }
            
            break;
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if the passed token repeats passed count times.
    /// </summary>
    protected bool CheckSequential(TTokenType tokenType, int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (!Check(i, tokenType))
            {
                return false;
            }
        }

        return true;
    }
    
    /// <summary>
    /// Check if the next tokens sequence is equal to the passed.
    /// </summary>
    protected bool CheckSequential(params TTokenType[] tokenTypes)
    {
        for (var i = 0; i < tokenTypes.Length; i++)
        {
            if (!Check(i, tokenTypes[i]))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Returns true when all tokens parsed.
    /// </summary>
    protected bool IsParseCompleted => Peek().TokenType == null;
    
    /// <summary>
    /// Returns current token.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">There is no current token.</exception>
    /// <returns></returns>
    protected Token<TTokenType> Peek() => tokens[_current];

    /// <summary>
    /// Switch to the next token and returns previous.
    /// </summary>
    /// <returns></returns>
    protected Token<TTokenType> Advance()
    {
        if (!IsParseCompleted)
            _current++;

        return Previous();
    }
    
    /// <summary>
    /// Move to the next position. Repeat passed count times.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    protected Token<TTokenType> Advance(int count)
    {
        Token<TTokenType>? result = null;
        for (var i = 0; i < count; i++)
        {
            result = Advance();
        }

        return result!;
    }
    
    /// <summary>
    /// Returns previous parsed token.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">The Previous token does not exist.</exception>
    /// <returns></returns>
    protected Token<TTokenType> Previous() => tokens[_current - 1];
    
    /// <summary>
    /// Returns true when the previous token exists.
    /// </summary>
    /// <returns></returns>
    protected bool HasPrevious() => _current > 0;
    
    private ParseException Error(Token<TTokenType> token, string message)
    {
        _errors.Add(new ParseError<TTokenType>
        {
            Error = message,
            Token = token,
        });

        return new ParseException();
    }
}