using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body;

public class TableMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required MarkdownContentBlockElement[] InnerElements { get; init; }
}