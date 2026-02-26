using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Blocks;

public class CodeMarkdownContentBlock : MarkdownContentBlock
{
    public required MarkdownContentBlockElement[] Elements { get; init; }
    public required string? Language { get; init; }
}