using System.Text;

namespace Laraue.Interpreter.Markdown.Body;

public class MarkdownTreeWriter
{
    private static readonly Dictionary<char, string> EscapedChars = new()
    {
        ['<'] = "&lt;",
        ['>'] = "&gt;",
    };
    
    public string Write(MarkdownTree tree)
    {
        var sb = new StringBuilder();
        
        foreach (var contentBlock in tree.ContentBlocks)
            Write(sb, contentBlock);

        return sb.ToString();
    }

    private void Write(StringBuilder sb, MarkdownContentBlock contentBlock)
    {
        switch (contentBlock)
        {
            case PlainMarkdownContentBlock plainBlock:
                Write(sb, plainBlock);
                break;
            case CodeMarkdownContentBlock codeBlock:
                Write(sb, codeBlock);
                break;
            case HeadingMarkdownContentBlock headingBlock:
                Write(sb, headingBlock);
                break;
            default:
                throw new NotImplementedException(contentBlock.GetType().Name);
        }
    }
    
    private void Write(StringBuilder sb, PlainMarkdownContentBlock contentBlock)
    {
        WriteElements(sb, "p", contentBlock.Elements);
    }
    
    private void Write(StringBuilder sb, CodeMarkdownContentBlock codeBlock)
    {
        sb.Append("<pre><code");
        
        if (codeBlock.Language != null)
            sb
                .Append(" class=\"")
                .Append(codeBlock.Language)
                .Append('\"');

        sb.Append('>');
        
        foreach (var innerElement in codeBlock.Elements)
            Write(sb, innerElement);

        sb.Append("</code></pre>");
    }
    
    private void Write(StringBuilder sb, HeadingMarkdownContentBlock headingBlock)
    {
        WriteElements(sb, $"h{headingBlock.Level}", headingBlock.Elements);
    }
    
    private void Write(StringBuilder sb, MarkdownContentBlockElement contentBlockElement)
    {
        switch (contentBlockElement)
        {
            case PlainMarkdownContentBlockElement plainBlockElement:
                Write(sb, plainBlockElement);
                break;
            case ItalicMarkdownContentBlockElement italicBlockElement:
                Write(sb, italicBlockElement);
                break;
            case BoldMarkdownContentBlockElement boldBlockElement:
                Write(sb, boldBlockElement);
                break;
            case InlineCodeMarkdownContentBlockElement codeBlockElement:
                Write(sb, codeBlockElement);
                break;
            case LinkCodeMarkdownContentBlockElement linkElement:
                Write(sb, linkElement);
                break;
            case ImageCodeMarkdownContentBlockElement imageElement:
                Write(sb, imageElement);
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
    private void Write(StringBuilder sb, PlainMarkdownContentBlockElement plainElement)
    {
        foreach (var ch in plainElement.Content)
        {
            if (EscapedChars.TryGetValue(ch, out var escapedChar))
                sb.Append(escapedChar);
            else
                sb.Append(ch);
        }
    }
    
    private void Write(StringBuilder sb, ItalicMarkdownContentBlockElement italicElement)
    {
        WriteElements(sb, "em", italicElement.InnerElements);
    }
    
    private void Write(StringBuilder sb, BoldMarkdownContentBlockElement italicElement)
    {
        WriteElements(sb, "b", italicElement.InnerElements);
    }
    
    private void Write(StringBuilder sb, InlineCodeMarkdownContentBlockElement codeElement)
    {
        WriteElements(sb, "code", codeElement.InnerElements);
    }
    
    private void Write(StringBuilder sb, LinkCodeMarkdownContentBlockElement linkElement)
    {
        sb.Append("<a");
        
        WriteAttribute(sb, "href", linkElement.Href);
        
        sb.Append('>')
            .Append(linkElement.Title)
            .Append("</a>");
    }
    
    private void Write(StringBuilder sb, ImageCodeMarkdownContentBlockElement imageElement)
    {
        sb.Append("<img");
        
        WriteAttribute(sb, "src", imageElement.Src);
        WriteAttribute(sb, "title", imageElement.Title);
        WriteAttribute(sb, "alt", imageElement.Alt);
        
        sb.Append(" />");
    }

    private void WriteAttribute(
        StringBuilder sb,
        string attributeName,
        string? attributeValue)
    {
        if (attributeValue != null)
            sb.Append($" {attributeName}=\"")
                .Append(attributeValue)
                .Append('"');
    }
    
    private void WriteElements(
        StringBuilder sb,
        string wrappingTag,
        MarkdownContentBlockElement[] elements)
    {
        sb.Append($"<{wrappingTag}>");
        
        foreach (var innerElement in elements)
            Write(sb, innerElement);

        sb.Append($"</{wrappingTag}>");
    }
}