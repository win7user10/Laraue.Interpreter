namespace Laraue.Interpreter.Markdown.Body;

public abstract class MarkdownContentBlock
{
}

public class HeadingMarkdownContentBlock : MarkdownContentBlock
{
    public required int Level { get; init; }
    
    public required MarkdownContentBlockElement[] Elements { get; init; }
}

public class PlainMarkdownContentBlock : MarkdownContentBlock
{
    public required MarkdownContentBlockElement[] Elements { get; set; }
}

public class CodeMarkdownContentBlock : MarkdownContentBlock
{
    public required MarkdownContentBlockElement[] Elements { get; init; }
    public required string? Language { get; init; }
}