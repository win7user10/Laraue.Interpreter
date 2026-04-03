using Laraue.Interpreter.Markdown.Body;
using Laraue.Interpreter.Markdown.Body.BlockElements;
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
                    Elements = [new LinkCodeMarkdownContentBlockElement
                    {
                        Href = "https://test.com",
                        Link =
                        [
                            new PlainMarkdownContentBlockElement
                            {
                                Content = "My link",
                            }
                        ]
                    }],
                }
            ]
        };

        var links = _generator.ParseLinks(tree);
        Assert.Equal(2, links.Count);
        
        Assert.Equal("#title", links.ElementAt(0).Link);
        Assert.Equal("Title", links.ElementAt(0).Title);
        
        Assert.Equal("#my-link", links.ElementAt(1).Link);
        Assert.Equal("My link", links.ElementAt(1).Title);
    }

    private PlainMarkdownContentBlockElement GetPlainElement(string literal)
    {
        return new PlainMarkdownContentBlockElement
        {
            Content = literal
        };
    }
}