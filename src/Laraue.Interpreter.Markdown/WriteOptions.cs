namespace Laraue.Interpreter.Markdown;

/// <summary>
/// Options to generate HTML from Markdown.
/// </summary>
public class WriteOptions
{
    /// <summary>
    /// When enabled each heading has identifier. Useful to make inner links in article.
    /// </summary>
    public bool AddIdAttributeToHeaders { get; set; }
}