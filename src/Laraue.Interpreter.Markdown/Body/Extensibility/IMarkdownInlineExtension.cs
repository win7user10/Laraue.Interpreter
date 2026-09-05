using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Extensibility;

/// <summary>
/// A pluggable inline markdown syntax, e.g. a custom span like <c>!video[alt](src)</c>.
/// Registered extensions are consulted before the built-in inline syntaxes (emphasis, links, etc.),
/// so an extension can also be used to change how an existing token sequence is interpreted.
/// </summary>
public interface IMarkdownInlineExtension
{
    /// <summary>
    /// Returns true if, at the parser's current position, this extension recognizes the start
    /// of its custom syntax and should be given a chance to parse it via <see cref="Read"/>.
    /// Must not consume any tokens.
    /// </summary>
    bool CanRead(IMarkdownInlineParserContext context);

    /// <summary>
    /// Parses the custom syntax starting at the parser's current position and returns the
    /// resulting AST element. Only called immediately after <see cref="CanRead"/> returned true.
    /// </summary>
    MarkdownContentBlockElement Read(IMarkdownInlineParserContext context);
}
