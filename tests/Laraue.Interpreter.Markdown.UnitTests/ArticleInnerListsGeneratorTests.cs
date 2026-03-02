using Laraue.Interpreter.Markdown.Body;
using Laraue.Interpreter.Markdown.Body.Blocks;

namespace Laraue.Interpreter.Markdown.UnitTests;

public class ArticleInnerListsGeneratorTests
{
    private readonly MarkdownInnerLinksGenerator _generator = new ();
    
    [Fact]
    public void Links_ShouldBeGenerated_Always()
    {
        var tree = new MarkdownTree
        {
            ContentBlocks = 
            [
                new HeadingMarkdownContentBlock
                {
                    Level = 1, 
                    Elements = [GetPlainElement("Title")],
                },
                new HeadingMarkdownContentBlock
                {
                    Level = 2, 
                    Elements = [GetPlainElement("Subtitle")],
                },
            ]
        };

        var links = _generator.ParseLinks(tree);
        Assert.Equal(2, links.Count);
    }

    private PlainMarkdownContentBlockElement GetPlainElement(string literal)
    {
        return new PlainMarkdownContentBlockElement
        {
            Content = literal
        };
    }
}