using Laraue.Interpreter.Markdown.Meta;

namespace Laraue.Interpreter.Markdown;

public class MarkdownTranspileResult
{
    public required string HtmlContent { get; init; }
    public required MarkdownHeader[] Headers { get; init; }
    
    /// <summary>
    /// The property fills when <see cref="WriteOptions.GenerateHeaderLinks"/> is set to true.
    /// </summary>
    public required MarkdownInnerLink[] InnerLinks { get; init; }
}