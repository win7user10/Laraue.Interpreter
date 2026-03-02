using System.Text;
using Laraue.Interpreter.Markdown.Body;
using Laraue.Interpreter.Markdown.Body.Blocks;

namespace Laraue.Interpreter.Markdown;

public interface IMarkdownInnerLinksGenerator
{
    ICollection<MarkdownInnerLink> ParseLinks(MarkdownTree markdownExpression);
}

public class MarkdownInnerLinksGenerator : IMarkdownInnerLinksGenerator
{
    public ICollection<MarkdownInnerLink> ParseLinks(MarkdownTree markdownExpression)
    {
        var allLinks = new List<MarkdownInnerLink>();
        
        foreach (var headingBlock in markdownExpression.ContentBlocks.OfType<HeadingMarkdownContentBlock>())
        {
            var linkBuilder = new StringBuilder();

            foreach (var plainElement in headingBlock.Elements.OfType<PlainMarkdownContentBlockElement>())
                linkBuilder.Append(plainElement.Content);
            
            var linkId = HeadingUtility
                .GenerateHeadingId(linkBuilder)
                .Insert(0, '#');
            
            allLinks.Add(new MarkdownInnerLink
            {
                Link = linkId.ToString(),
                Title = linkBuilder.ToString(),
                Level = headingBlock.Level,
            });
        }
        
        return allLinks;
    }
}