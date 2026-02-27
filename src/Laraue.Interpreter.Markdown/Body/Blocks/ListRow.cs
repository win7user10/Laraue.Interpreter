using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Blocks;

public class ListRow
{
    public required MarkdownContentBlockElement[] Elements { get; init; }
    public List<ListRow> Children { get; } = [];
}