namespace Laraue.Interpreter.Markdown;

public class MarkdownInnerLink
{
    public required int Level { get; init; }
    public required string Title { get; init; }
    public required string Link { get; init; }
}