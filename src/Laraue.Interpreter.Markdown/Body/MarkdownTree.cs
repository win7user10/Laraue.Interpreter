using Laraue.Interpreter.Markdown.Body.Blocks;

namespace Laraue.Interpreter.Markdown.Body;

public class MarkdownTree
{
    public required MarkdownContentBlock[] ContentBlocks { get; init; }
}