namespace Laraue.Interpreter.Scanning;

/// <summary>
/// Base class to implement a <see href="https://craftinginterpreters.com/scanning.html">Scanner</see>.
/// </summary>
/// <param name="input">The string to scan.</param>
/// <typeparam name="TTokenType">Type of tokens should be returned.</typeparam>
public abstract class TokenScanner<TTokenType>(string input) where TTokenType : struct, Enum
{
    /// <summary>
    /// Input string.
    /// </summary>
    protected readonly string Input = input;

    private int _startAbsolutePosition;
    private int _currentAbsolutePosition;
    private int _startRelativePosition;
    private int _currentRelativePosition;
    private int _lineNumber;
    private readonly List<Token<TTokenType>> _tokens = new ();
    private readonly List<ScanError> _errors = new ();

    /// <summary>
    /// The main function to implement in scanner.
    /// It should either process character and return true or return false that mean the character is unprocessed.
    /// </summary>
    /// <returns></returns>
    protected abstract bool TryProcessNextChar(char nextChar);
    
    /// <summary>
    /// Run the scanner and returns the scanning result.
    /// </summary>
    /// <returns></returns>
    public ScanResult<TTokenType> ScanTokens()
    {
        while (!IsScanCompleted)
        {
            _startAbsolutePosition = _currentAbsolutePosition;
            _startRelativePosition = _currentRelativePosition;
            ScanToken();
        }
            
        _tokens.Add(new Token<TTokenType>
        {
            TokenType = null,
            Lexeme = null,
            StartPosition = _startRelativePosition,
            EndPosition = _currentRelativePosition,
            LineNumber = _lineNumber,
        });
            
        return new ScanResult<TTokenType>
        {
            Tokens = _tokens.ToArray(),
            Errors = _errors.ToArray()
        };
    }
    
    /// <summary>
    /// Returns true when the character with the specified offset is equal to passed.
    /// </summary>
    /// <returns></returns>
    protected bool Check(int offset, char charToCheck)
    {
        return Check(offset, x => x == charToCheck);
    }
    
    /// <summary>
    /// Returns true if the character with the specified offset passes the predicate.
    /// </summary>
    /// <returns></returns>
    protected bool Check(int offset, Func<char, bool> check)
    {
        if (Input.Length > _currentAbsolutePosition + offset)
        {
            return check(Input[_currentAbsolutePosition + offset]);
        }
        
        return false;
    }
    
    /// <summary>
    /// Move the scanner to the next position and returns the token.
    /// <exception cref="IndexOutOfRangeException">The character with the next offset does not exist.</exception>
    /// </summary>
    protected char Advance()
    {
        _currentRelativePosition++;
        return Input[_currentAbsolutePosition++];
    }
    
    /// <summary>
    /// Move the scanner to the next position if next position character satisfies the passed predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    protected bool PopNextCharIf(Func<char, bool> predicate)
    {
        if (IsScanCompleted)
        {
            return false;
        }

        if (!predicate(Input[_currentAbsolutePosition]))
        {
            return false;
        }
            
        _currentAbsolutePosition++;
        _currentRelativePosition++;
        return true;
    }
    
    /// <summary>
    /// The function should be called when the next line starts scanning. Required to get correct error messages.
    /// </summary>
    protected void ToNextLine()
    {
        _lineNumber++;
        _currentRelativePosition = 0;
    }
    
    /// <summary>
    /// Add the scanned token to the result.
    /// </summary>
    /// <param name="tokenType">Type of scanned token.</param>
    /// <param name="literal">The value of scanned tokens (for strings, numbers, etc.)</param>
    protected void AddToken(TTokenType tokenType, object? literal = null)
    {
        var lexeme = Input[_startAbsolutePosition.._currentAbsolutePosition];
            
        _tokens.Add(new Token<TTokenType>
        {
            TokenType = tokenType,
            Lexeme = lexeme,
            Literal = literal,
            StartPosition = _startRelativePosition,
            EndPosition = _currentRelativePosition,
            LineNumber = _lineNumber
        });
    }
    
    /// <summary>
    /// Returns true if the character is digit.
    /// </summary>
    /// <returns></returns>
    protected bool IsDigit (char c) => c is >= '0' and <= '9';
    
    /// <summary>
    /// Returns true if the character is a word letter.
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    protected bool IsAlpha (char c) => c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
    
    /// <summary>
    /// Returns true when the scanning is fully completed.
    /// </summary>
    protected bool IsScanCompleted => _currentAbsolutePosition >= Input.Length;

    /// <summary>
    /// Returns all characters that scanned in the current iteration. Usable for cases where token literal is required.
    /// </summary>
    /// <returns></returns>
    protected string GetCurrentScanValue()
    {
        return Input[_startAbsolutePosition.._currentAbsolutePosition];
    }
    
    private void ScanToken()
    {
        var nextChar = Advance();
        if (!TryProcessNextChar(nextChar))
        {
            _errors.Add(new ScanError
            {
                StartPosition = _startRelativePosition,
                EndPosition = _currentRelativePosition,
                Error = $"Unknown character '{nextChar}'.",
                LineNumber = _lineNumber
            });
        }
    }
}