namespace Laraue.Interpreter.Markdown.Body.Blocks;

public class ListBlock : MarkdownContentBlock
{
    public required ListRow[] Rows { get; init; }
    public bool IsOrdered { get; init; }
}