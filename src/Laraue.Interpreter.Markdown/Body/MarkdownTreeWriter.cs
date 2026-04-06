using System.Text;
using Laraue.Interpreter.Markdown.Body.BlockElements;
using Laraue.Interpreter.Markdown.Body.Blocks;

namespace Laraue.Interpreter.Markdown.Body;

public class MarkdownTreeWriter
{
    private readonly WriteOptions _options;

    public MarkdownTreeWriter(WriteOptions options)
    {
        _options = options;
    }
    
    private static readonly Dictionary<char, string> EscapedChars = new()
    {
        ['<'] = "&lt;",
        ['>'] = "&gt;",
    };
    
    public string Write(MarkdownTree tree)
    {
        var sb = new IndentedStringBuilder();
        
        foreach (var contentBlock in tree.ContentBlocks)
            Write(sb, contentBlock);

        return sb.ToString();
    }

    private void Write(IndentedStringBuilder sb, MarkdownContentBlock contentBlock)
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
            case TableContentBlock tableBlock:
                Write(sb, tableBlock);
                break;
            case ListBlock orderedListBlock:
                Write(sb, orderedListBlock);
                break;
            case BlockquoteContentBlock blockquoteContentBlock:
                Write(sb, blockquoteContentBlock);
                break;
            case HrContentBlock hrContentBlock:
                Write(sb, hrContentBlock);
                break;
            default:
                throw new NotImplementedException(contentBlock.GetType().Name);
        }
    }
    
    private void Write(IndentedStringBuilder sb, PlainMarkdownContentBlock contentBlock)
    {
        WriteElements(sb, "p", contentBlock.Elements);
    }
    
    private void Write(IndentedStringBuilder sb, CodeMarkdownContentBlock codeBlock)
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
    
    private void Write(IndentedStringBuilder sb, HeadingMarkdownContentBlock headingBlock)
    {
        sb.Append($"<h{headingBlock.Level}");
        if (_options.GenerateHeaderLinks)
        {
            var heading = HeadingUtility.GenerateHeading(headingBlock.Elements);
            sb
                .Append(" id=\"")
                .Append(heading.Id)
                .Append('"');
        }

        sb.Append('>');
        
        foreach (var innerElement in headingBlock.Elements)
            Write(sb, innerElement); 
        
        sb.Append($"</h{headingBlock.Level}>");
    }
    
    private void Write(IndentedStringBuilder sb, TableContentBlock tableBlock)
    {
        sb
            .Append("<table>");

        if (tableBlock.Header is not null)
        {
            sb.Append("<thead>")
                .Append("<tr>");

            foreach (var row in tableBlock.Header.Cells)
                WriteElements(sb, "th", row.Elements);

            sb
                .Append("</tr>")
                .Append("</thead>");
        }
        
        sb.Append("<tbody>");

        foreach (var row in tableBlock.Rows)
        {
            sb.Append("<tr>");
            foreach (var cell in row.Cells)
                WriteElements(sb, "td", cell.Elements);
            sb.Append("</tr>");
        }
        
        sb
            .Append("</tbody>")
            .Append("</table>");
    }
    
    private void Write(IndentedStringBuilder sb, ListBlock listBlock)
    {
        var tag = listBlock.IsOrdered ? "ol" : "ul";
        WriteListRow(sb, listBlock.Rows, tag);
    }

    private void WriteListRow(IndentedStringBuilder stringBuilder, IEnumerable<ListRow> rows, string tag)
    {
        stringBuilder
            .Append($"<{tag}>");

        foreach (var row in rows)
        {
            if (row.Elements.Length > 0)
                WriteElements(stringBuilder, "li", row.Elements);
            if (row.Children.Count > 0)
                WriteListRow(stringBuilder, row.Children, tag);
        }
            
        stringBuilder
            .Append($"</{tag}>");
    }
    
    private void Write(IndentedStringBuilder sb, BlockquoteContentBlock blockquoteContentBlock)
    {
        sb.Append("<blockquote><p>");

        for (var index = 0; index < blockquoteContentBlock.Elements.Count; index++)
        {
            var innerElement = blockquoteContentBlock.Elements[index];
            foreach (var element in innerElement)
                Write(sb, element);
            
            if (index < blockquoteContentBlock.Elements.Count - 1)
                sb.Append("<br>");
        }

        sb.Append("</p></blockquote>");
    }
    
    private void Write(IndentedStringBuilder sb, MarkdownContentBlockElement contentBlockElement)
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
            case NewLineElement newLineElement:
                Write(sb, newLineElement);
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
    private void Write(IndentedStringBuilder sb, PlainMarkdownContentBlockElement plainElement)
    {
        foreach (var ch in plainElement.Content)
        {
            if (EscapedChars.TryGetValue(ch, out var escapedChar))
                sb.Append(escapedChar);
            else
                sb.Append(ch);
        }
    }
    
    private void Write(IndentedStringBuilder sb, ItalicMarkdownContentBlockElement italicElement)
    {
        WriteElements(sb, "em", italicElement.InnerElements);
    }
    
    private void Write(IndentedStringBuilder sb, BoldMarkdownContentBlockElement italicElement)
    {
        WriteElements(sb, "b", italicElement.InnerElements);
    }
    
    private void Write(IndentedStringBuilder sb, InlineCodeMarkdownContentBlockElement codeElement)
    {
        WriteElements(sb, "code", codeElement.InnerElements);
    }
    
    private void Write(IndentedStringBuilder sb, LinkCodeMarkdownContentBlockElement linkElement)
    {
        sb.Append("<a");
        
        WriteAttribute(sb, "href", linkElement.Href);
        
        sb.Append('>');
        
        foreach (var innerElement in linkElement.Link)
            Write(sb, innerElement);
        
        sb.Append("</a>");
    }
    
    private void Write(IndentedStringBuilder sb, ImageCodeMarkdownContentBlockElement imageElement)
    {
        sb.Append("<img");
        
        WriteAttribute(sb, "src", imageElement.Src);
        WriteAttribute(sb, "title", imageElement.Title);
        WriteAttribute(sb, "alt", imageElement.Alt);
        
        sb.Append(" />");
    }
    
    private void Write(IndentedStringBuilder sb, HrContentBlock hrBlock)
    {
        sb.Append("<hr>");
    }
    
    private void Write(IndentedStringBuilder sb, NewLineElement newLineElement)
    {
        sb.AppendLine();
    }

    private void WriteAttribute(
        IndentedStringBuilder sb,
        string attributeName,
        string? attributeValue)
    {
        if (attributeValue != null)
            sb.Append($" {attributeName}=\"")
                .Append(attributeValue)
                .Append('"');
    }
    
    private void WriteElements(
        IndentedStringBuilder sb,
        string wrappingTag,
        IEnumerable<MarkdownContentBlockElement> elements)
    {
        sb.Append($"<{wrappingTag}>");

        foreach (var innerElement in elements)
            Write(sb, innerElement);

        sb.Append($"</{wrappingTag}>");
    }
}