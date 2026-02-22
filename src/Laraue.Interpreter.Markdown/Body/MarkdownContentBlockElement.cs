namespace Laraue.Interpreter.Markdown.Body;

public abstract class MarkdownContentBlockElement
{
}

public class ItalicMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required MarkdownContentBlockElement[] InnerElements { get; init; }
}

public class BoldMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required MarkdownContentBlockElement[] InnerElements { get; init; }
}

public class PlainMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required string Content { get; init; }
}