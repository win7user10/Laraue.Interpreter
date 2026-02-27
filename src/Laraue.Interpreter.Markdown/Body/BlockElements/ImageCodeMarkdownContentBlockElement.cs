namespace Laraue.Interpreter.Markdown.Body.BlockElements;

public class ImageCodeMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required string? Src { get; init; }
    public required string? Alt { get; init; }
    public required string? Title { get; init; }
}