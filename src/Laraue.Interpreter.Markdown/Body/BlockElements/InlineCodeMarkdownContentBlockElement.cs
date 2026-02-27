namespace Laraue.Interpreter.Markdown.Body.BlockElements;

public class InlineCodeMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required MarkdownContentBlockElement[] InnerElements { get; init; }
}