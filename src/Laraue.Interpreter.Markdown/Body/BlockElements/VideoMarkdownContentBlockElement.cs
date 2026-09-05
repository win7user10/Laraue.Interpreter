namespace Laraue.Interpreter.Markdown.Body.BlockElements;

/// <summary>
/// AST node for the <c>!video[alt](src)</c> syntax added by <see cref="Extensibility.Examples.VideoMarkdownExtension"/>.
/// </summary>
public class VideoMarkdownContentBlockElement : MarkdownContentBlockElement
{
    public required string? Src { get; init; }
    public required string? Alt { get; init; }
}
