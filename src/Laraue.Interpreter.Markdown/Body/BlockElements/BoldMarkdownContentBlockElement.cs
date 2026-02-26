namespace Laraue.Interpreter.Markdown.Body.BlockElements;

public class BoldMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required MarkdownContentBlockElement[] InnerElements { get; init; }
}