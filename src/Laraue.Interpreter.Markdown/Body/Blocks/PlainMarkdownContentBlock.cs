using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Blocks;

public class PlainMarkdownContentBlock : MarkdownContentBlock
{
    public required MarkdownContentBlockElement[] Elements { get; set; }
}