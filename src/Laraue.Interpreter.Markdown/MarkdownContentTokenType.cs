namespace Laraue.Interpreter.Markdown;

public enum MarkdownContentTokenType
{
    /// <summary>
    /// '*'
    /// </summary>
    Asterisk,
    
    /// <summary>
    /// '**'
    /// </summary>
    DoubleAsterisk,
    
    /// <summary>
    /// '`'
    /// </summary>
    Backtick,
    
    /// <summary>
    /// '#'
    /// </summary>
    NumberSign,
    
    /// <summary>
    /// '['
    /// </summary>
    LeftSquareBracket,
    
    /// <summary>
    /// ']'
    /// </summary>
    RightSquareBracket,
    
    /// <summary>
    /// '('
    /// </summary>
    LeftParenthesis,
    
    /// <summary>
    /// ')'
    /// </summary>
    RightParenthesis,
    
    /// <summary>
    /// '-'
    /// </summary>
    MinusSign,
    
    /// <summary>
    /// ' '
    /// </summary>
    Whitespace,
    
    /// <summary>
    /// '\r\n'
    /// </summary>
    NewLine,
    
    /// <summary>
    /// '(\w\d)+'
    /// </summary>
    Word,
    
    /// <summary>
    /// '_'
    /// </summary>
    Underscore,
    
    /// <summary>
    /// '__'
    /// </summary>
    DoubleUnderscore,
    
    /// <summary>
    /// (\d+).(\d+)
    /// </summary>
    Number,
    
    /// <summary>
    /// '"'
    /// </summary>
    Quote,
}