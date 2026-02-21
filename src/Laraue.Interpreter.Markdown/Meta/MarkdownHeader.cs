namespace Laraue.Interpreter.Markdown.Meta;

public record MarkdownHeader
{
    public required string PropertyName { get; set; }
    public required object? Value { get; set; }
    public required int LineNumber { get; set; }
}