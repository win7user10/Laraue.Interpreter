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

        for (var index = 0; index < tree.ContentBlocks.Length; index++)
        {
            var contentBlock = tree.ContentBlocks[index];
            Write(sb, contentBlock);
            if (index < tree.ContentBlocks.Length - 1)
                sb.AppendNewLine();
        }

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
        sb.Append("<pre>");
        
        sb.WithIdent(inner =>
        {
            inner.Append("<code");
            if (codeBlock.Language != null)
                inner
                    .Append(" class=\"")
                    .Append(codeBlock.Language)
                    .Append('\"');

            inner.Append('>');
        
            foreach (var innerElement in codeBlock.Elements)
                Write(inner, innerElement);
            
            inner.Append("</code>");
        });

        sb.AppendNewLine("</pre>");
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
        
        sb.WithIdent(inner =>
        {
            foreach (var innerElement in headingBlock.Elements)
                Write(inner, innerElement); 
        });
        
        sb.AppendNewLine($"</h{headingBlock.Level}>");
    }
    
    private void Write(IndentedStringBuilder sb, TableContentBlock tableBlock)
    {
        sb.Append("<table>");

        sb.WithIdent(tableBuilder =>
        {
            if (tableBlock.Header is not null)
            {
                tableBuilder.Append("<thead>");
                tableBuilder.WithIdent(headBuilder =>
                {
                    headBuilder.Append("<tr>");
                    headBuilder.WithIdent(rowBuilder =>
                    {
                        for (var index = 0; index < tableBlock.Header.Cells.Length; index++)
                        {
                            var row = tableBlock.Header.Cells[index];
                            WriteElements(rowBuilder, "th", row.Elements);
                            if (index < tableBlock.Header.Cells.Length - 1)
                                rowBuilder.AppendNewLine();
                        }
                    });
                    
                    headBuilder.AppendNewLine("</tr>");
                });
                
                tableBuilder
                    .AppendNewLine("</thead>")
                    .AppendNewLine();
            }
        
            tableBuilder.Append("<tbody>");
            tableBuilder.WithIdent(bodyBuilder =>
            {
                for (var i = 0; i < tableBlock.Rows.Length; i++)
                {
                    var row = tableBlock.Rows[i];
                    bodyBuilder.Append("<tr>");
                    bodyBuilder.WithIdent(rowBuilder =>
                    {
                        for (var index = 0; index < row.Cells.Length; index++)
                        {
                            var cell = row.Cells[index];
                            WriteElements(rowBuilder, "td", cell.Elements);
                            if (index < row.Cells.Length - 1)
                                rowBuilder.AppendNewLine();
                        }
                    });

                    bodyBuilder.AppendNewLine("</tr>");
                    if (i < tableBlock.Rows.Length - 1)
                        bodyBuilder.AppendNewLine();
                }
            });
            
            sb.AppendNewLine("</tbody>");
        });
        
        sb.AppendNewLine("</table>");
    }
    
    private void Write(IndentedStringBuilder sb, ListBlock listBlock)
    {
        var tag = listBlock.IsOrdered ? "ol" : "ul";
        WriteListRow(sb, listBlock.Rows, tag);
    }

    private void WriteListRow(IndentedStringBuilder stringBuilder, IReadOnlyCollection<ListRow> rows, string tag)
    {
        stringBuilder
            .Append($"<{tag}>");

        stringBuilder.WithIdent(inner =>
        {
            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows.ElementAt(index);
                if (row.Elements.Length > 0)
                {
                    WriteElements(inner, "li", row.Elements);
                    if (index < rows.Count - 1)
                        inner.AppendNewLine();
                }

                if (row.Children.Count > 0)
                {
                    inner.AppendNewLine();
                    WriteListRow(inner, row.Children, tag);
                    if (index < rows.Count - 1)
                        inner.AppendNewLine();
                }
                
            }
        });
            
        stringBuilder
            .AppendNewLine($"</{tag}>");
    }
    
    private void Write(IndentedStringBuilder sb, BlockquoteContentBlock blockquoteContentBlock)
    {
        sb.Append("<blockquote>");

        sb.WithIdent(inner =>
        {
            inner.Append("<p>");
            inner.WithIdent(subInner =>
            {
                for (var index = 0; index < blockquoteContentBlock.Elements.Count; index++)
                {
                    var innerElement = blockquoteContentBlock.Elements[index];
                    foreach (var element in innerElement)
                        Write(subInner, element);
            
                    if (index < blockquoteContentBlock.Elements.Count - 1)
                        subInner.Append("<br>");
                }
            });
            inner.AppendNewLine("</p>");
        });
        
        sb.AppendNewLine("</blockquote>");
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
        sb.AppendNewLine();
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
        var elementsArray = elements.ToArray();
        
        sb.Append($"<{wrappingTag}>");
        if (elementsArray.Length == 0)
        {
            sb.Append($"</{wrappingTag}>");
            return;
        }
        
        sb.WithIdent(ident =>
        {
            foreach (var innerElement in elementsArray)
                Write(ident, innerElement);
        });
            
        sb.AppendNewLine($"</{wrappingTag}>");
    }
}