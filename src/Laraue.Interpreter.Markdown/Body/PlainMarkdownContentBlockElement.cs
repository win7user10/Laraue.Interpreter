using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body;

public class PlainMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required string Content { get; set; }
}