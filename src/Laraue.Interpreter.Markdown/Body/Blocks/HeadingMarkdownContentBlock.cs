using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Blocks;

public class HeadingMarkdownContentBlock : MarkdownContentBlock
{
    public required int Level { get; init; }
    
    public required MarkdownContentBlockElement[] Elements { get; init; }
}