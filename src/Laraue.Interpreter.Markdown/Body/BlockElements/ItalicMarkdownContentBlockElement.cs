namespace Laraue.Interpreter.Markdown.Body.BlockElements;

public class ItalicMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required MarkdownContentBlockElement[] InnerElements { get; init; }
}