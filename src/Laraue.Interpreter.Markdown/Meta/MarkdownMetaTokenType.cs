namespace Laraue.Interpreter.Markdown.Meta;

public enum MarkdownMetaTokenType
{
    /// <summary>
    /// "---"
    /// </summary>
    MetaDelimiter,
    
    /// <summary>
    /// Any chars sequence
    /// </summary>
    Word,
    
    /// <summary>
    /// ':'
    /// </summary>
    Delimiter,
    
    /// <summary>
    /// '['
    /// </summary>
    ArrayStart,
    
    /// <summary>
    /// ']'
    /// </summary>
    ArrayEnd,
    
    /// <summary>
    /// ','
    /// </summary>
    Comma,
    
    /// <summary>
    /// The whole Markdown file content.
    /// </summary>
    Content,
    
    NewLine,
    
    WhiteSpace,
}