using Laraue.Interpreter.Markdown.Body.BlockElements;
using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Markdown.Body.Extensibility;

/// <summary>
/// The parsing primitives exposed to <see cref="IMarkdownInlineExtension"/> implementations.
/// Mirrors the subset of <see cref="MarkdownTokenParser"/>'s own cursor operations that are
/// safe to expose to third-party inline syntax extensions.
/// </summary>
public interface IMarkdownInlineParserContext
{
    /// <summary>
    /// Returns true if the token at the current position is of the given type.
    /// </summary>
    bool Check(MarkdownTokenType tokenType);

    /// <summary>
    /// Returns true if the token at the given offset from the current position is of the given type.
    /// </summary>
    bool Check(int offset, MarkdownTokenType tokenType);

    /// <summary>
    /// Returns true if the token at the given offset from the current position is a <see cref="MarkdownTokenType.Word"/>
    /// token whose text is exactly equal to <paramref name="word"/> (case-sensitive, ordinal comparison).
    /// </summary>
    bool CheckWord(int offset, string word);

    /// <summary>
    /// Returns true when the current row (line) has been fully consumed, or the whole input has been parsed.
    /// </summary>
    bool IsRowEndReached();

    /// <summary>
    /// Consumes and returns the token at the current position.
    /// </summary>
    Token<MarkdownTokenType> Advance();

    /// <summary>
    /// Parses one inline element starting at the current position, using the normal parsing rules
    /// (built-in emphasis/links/etc. as well as other registered extensions). Useful for extensions
    /// that want to allow nested inline content, e.g. inside a custom span's text.
    /// </summary>
    MarkdownContentBlockElement ReadElement();

    /// <summary>
    /// Consumes tokens one at a time, converting each to text, until a token of type <paramref name="terminator"/>
    /// is encountered (not consumed) or the row/input ends. Useful for reading raw runs of text such as
    /// a link href or an image source.
    /// </summary>
    string ReadRawTextUntil(MarkdownTokenType terminator);
}
