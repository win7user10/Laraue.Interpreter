using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Extensibility;

/// <summary>
/// Renders the AST element(s) produced by a matching <see cref="IMarkdownInlineExtension"/> to HTML.
/// </summary>
public interface IMarkdownElementWriterExtension
{
    /// <summary>
    /// Returns true if this extension knows how to render the given element.
    /// </summary>
    bool CanWrite(MarkdownContentBlockElement element);

    /// <summary>
    /// Writes the given element. Only called immediately after <see cref="CanWrite"/> returned true.
    /// </summary>
    void Write(IMarkdownWriterContext context, MarkdownContentBlockElement element);
}
