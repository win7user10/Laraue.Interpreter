using System.Text;

namespace Laraue.Interpreter.Markdown.Body;

public class MarkdownTreeWriter
{
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
            default:
                throw new NotImplementedException();
        }
    }
    
    private void Write(StringBuilder sb, PlainMarkdownContentBlock contentBlock)
    {
        sb.Append("<p>");
        
        foreach (var element in contentBlock.Elements)
            Write(sb, element);

        sb.Append("</p>");
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
            default:
                throw new NotImplementedException();
        }
    }
    
    private void Write(StringBuilder sb, PlainMarkdownContentBlockElement plaintElement)
    {
        sb.Append(plaintElement.Content);
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