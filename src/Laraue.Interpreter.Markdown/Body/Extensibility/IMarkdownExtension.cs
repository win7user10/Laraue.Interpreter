namespace Laraue.Interpreter.Markdown.Body.Extensibility;

/// <summary>
/// Bundles a custom inline markdown syntax together with the logic to render it, so that a single
/// class can be registered with <see cref="Laraue.Interpreter.Markdown.MarkdownTranspiler"/> to
/// add support for new markdown syntax without modifying the core scanner/parser/writer.
/// </summary>
public interface IMarkdownExtension : IMarkdownInlineExtension, IMarkdownElementWriterExtension;
