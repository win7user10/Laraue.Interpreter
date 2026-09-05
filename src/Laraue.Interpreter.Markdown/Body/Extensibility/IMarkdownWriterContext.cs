using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Extensibility;

/// <summary>
/// The writing primitives exposed to <see cref="IMarkdownElementWriterExtension"/> implementations.
/// Mirrors the subset of <see cref="IndentedStringBuilder"/>'s own operations that are safe to
/// expose to third-party element writers.
/// </summary>
public interface IMarkdownWriterContext
{
    /// <summary>
    /// Appends a raw string to the output.
    /// </summary>
    void Append(string value);

    /// <summary>
    /// Appends a raw character to the output.
    /// </summary>
    void Append(char value);

    /// <summary>
    /// Starts a new output line, optionally with the given content.
    /// </summary>
    void AppendNewLine(string? value = null);

    /// <summary>
    /// Runs <paramref name="action"/> with the indentation level increased by one.
    /// </summary>
    void WithIdent(Action<IMarkdownWriterContext> action);

    /// <summary>
    /// Writes a nested element (built-in or from another extension) using the normal writing rules.
    /// Useful for extensions whose custom element wraps ordinary inline content.
    /// </summary>
    void WriteChild(MarkdownContentBlockElement element);
}
