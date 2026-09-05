namespace Laraue.Interpreter.Markdown.Body.BlockElements;

public class StrikethroughMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required MarkdownContentBlockElement[] InnerElements { get; init; }
}
