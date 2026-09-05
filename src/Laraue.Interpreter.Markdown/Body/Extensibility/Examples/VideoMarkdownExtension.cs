using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body.Extensibility.Examples;

/// <summary>
/// Example <see cref="IMarkdownExtension"/> adding a custom video syntax, modeled after the
/// built-in image syntax but with an explicit "video" marker so it's unambiguous regardless
/// of the file extension in <c>src</c>: <c>!video[alt text](src)</c> renders as
/// <c>&lt;video controls src="src"&gt;alt text&lt;/video&gt;</c>.
/// </summary>
public class VideoMarkdownExtension : IMarkdownExtension
{
    private const string Keyword = "video";

    /// <inheritdoc />
    public bool CanRead(IMarkdownInlineParserContext context)
    {
        return context.Check(MarkdownTokenType.Not)
            && context.CheckWord(1, Keyword)
            && context.Check(2, MarkdownTokenType.LeftSquareBracket);
    }

    /// <inheritdoc />
    public MarkdownContentBlockElement Read(IMarkdownInlineParserContext context)
    {
        context.Advance(); // '!'
        context.Advance(); // 'video'
        context.Advance(); // '['

        var alt = context.ReadRawTextUntil(MarkdownTokenType.RightSquareBracket);
        if (context.Check(MarkdownTokenType.RightSquareBracket))
            context.Advance();

        string? src = null;
        if (context.Check(MarkdownTokenType.LeftParenthesis))
        {
            context.Advance();
            src = context.ReadRawTextUntil(MarkdownTokenType.RightParenthesis);
            if (context.Check(MarkdownTokenType.RightParenthesis))
                context.Advance();
        }

        return new VideoMarkdownContentBlockElement
        {
            Alt = alt.Length > 0 ? alt : null,
            Src = src,
        };
    }

    /// <inheritdoc />
    public bool CanWrite(MarkdownContentBlockElement element) => element is VideoMarkdownContentBlockElement;

    /// <inheritdoc />
    public void Write(IMarkdownWriterContext context, MarkdownContentBlockElement element)
    {
        var video = (VideoMarkdownContentBlockElement)element;

        context.Append("<video controls");
        if (video.Src != null)
            context.Append($" src=\"{video.Src}\"");
        context.Append('>');

        if (video.Alt != null)
            context.Append(video.Alt);

        context.Append("</video>");
    }
}
