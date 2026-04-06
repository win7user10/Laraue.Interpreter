using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Blocks;

public class BlockquoteContentBlock : MarkdownContentBlock
{
    public required List<MarkdownContentBlockElement[]> Elements { get; init; }
}