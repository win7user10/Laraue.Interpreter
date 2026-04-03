using System.Text;
using Laraue.Interpreter.Markdown.Body;
using Laraue.Interpreter.Markdown.Body.BlockElements;
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
            var heading = HeadingUtility.GenerateHeading(headingBlock.Elements);
            
            var linkId = heading.Id
                .Insert(0, '#');
            
            allLinks.Add(new MarkdownInnerLink
            {
                Link = linkId.ToString(),
                Title = heading.Title,
                Level = headingBlock.Level,
            });
        }
        
        return allLinks;
    }
}