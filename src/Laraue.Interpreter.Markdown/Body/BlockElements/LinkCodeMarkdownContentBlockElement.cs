namespace Laraue.Interpreter.Markdown.Body.BlockElements;

public class LinkCodeMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required string? Href { get; init; }
    public required MarkdownContentBlockElement[] Link { get; init; }
}