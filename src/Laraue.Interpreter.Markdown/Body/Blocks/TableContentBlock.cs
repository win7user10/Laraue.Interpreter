using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Blocks;

public class TableContentBlock : MarkdownContentBlock
{
    public required TableContentBlockRow? Header { get; init; }
    public required TableContentBlockRow[] Rows { get; init; }
}

public class TableContentBlockRow
{
    public required TableContentBlockCell[] Cells { get; init; }
}

public class TableContentBlockCell
{
    public required MarkdownContentBlockElement[] Elements { get; set; }
}