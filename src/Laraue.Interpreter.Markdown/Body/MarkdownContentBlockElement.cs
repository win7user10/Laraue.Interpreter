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

public class InlineCodeMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required MarkdownContentBlockElement[] InnerElements { get; init; }
}

public class PlainMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required string Content { get; set; }
}

public class LinkCodeMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required string? Href { get; init; }
    public required string Title { get; init; }
}

public class ImageCodeMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required string? Src { get; init; }
    public required string? Alt { get; init; }
    public required string? Title { get; init; }
}