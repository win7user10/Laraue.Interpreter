using Laraue.Interpreter.Markdown.Meta;

namespace Laraue.Interpreter.Markdown;

public class MarkdownTranspileResult
{
    public required string HtmlContent { get; init; }
    public required MarkdownHeader[] Headers { get; init; }
}