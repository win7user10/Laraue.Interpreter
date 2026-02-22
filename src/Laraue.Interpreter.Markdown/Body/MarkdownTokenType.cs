namespace Laraue.Interpreter.Markdown.Body;

public enum MarkdownTokenType
{
    NewLine,
    Whitespace,
    Word,
    
    /// <summary>
    /// '*'
    /// </summary>
    Asterisk,
    
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
    /// '_'
    /// </summary>
    Underscore,
}