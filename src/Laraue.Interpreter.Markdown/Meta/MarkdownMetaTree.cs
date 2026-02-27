namespace Laraue.Interpreter.Markdown.Meta;

public class MarkdownMetaTree
{
    public required MarkdownHeader[] Headers { get; set; }
    public required string Content { get; set; }
}