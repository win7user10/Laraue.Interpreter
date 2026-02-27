using Laraue.Interpreter.Markdown.Meta;
using Laraue.Interpreter.Markdown.UnitTests.Infrastructure;

namespace Laraue.Interpreter.Markdown.UnitTests;

public class MarkdownMetaTokenParserTests
{
    [Fact]
    public void MarkdownMetaTokenParser_ShouldParseTree_WhenFormattingIsNormal()
    {
        const string markdownFile = @"---
tags: [tag1, tag2]
project: project1
type: unitTestArticle
---

# Title of level 1";
        
        var parseResult = Parse(markdownFile);
        Assert.Equal("# Title of level 1", parseResult.Content);

        var headers = parseResult.Headers;
        
        Assert.Equal(3, headers.Length);
        
        CheckHeaderUtility.Check(
            headers[0],
            "tags",
            1,
            new[] { "tag1", "tag2" });
        
        CheckHeaderUtility.Check(
            headers[1],
            "project",
            2,
            "project1");
        
        CheckHeaderUtility.Check(
            headers[2],
            "type",
            3,
            "unitTestArticle");
    }
    
    [Fact]
    public void MarkdownMetaTokenParser_ShouldParseTree_WhenFormattingIsAbnormal()
    {
        const string markdownFile = @"---

tags  : [tag1,  tag2 ]


project: project1
type: unitTestArticle
---

# Title of level 1";
        
        var parseResult = Parse(markdownFile);
        Assert.Equal("# Title of level 1", parseResult.Content);

        var headers = parseResult.Headers;
        
        Assert.Equal(3, headers.Length);
        
        CheckHeaderUtility.Check(
            headers[0],
            "tags",
            2,
            new[] { "tag1", "tag2" });
        
        CheckHeaderUtility.Check(
            headers[1],
            "project",
            5,
            "project1");
        
        CheckHeaderUtility.Check(
            headers[2],
            "type",
            6,
            "unitTestArticle");
    }

    private static MarkdownMetaTree Parse(string markdown)
    {
        var scanner = new MarkdownMetaTokenScanner(markdown);
        var scanResult = scanner.ScanTokens();

        var parser = new MarkdownMetaTokenParser(scanResult.Tokens);
        var parseResult = parser.Parse();
        
        Assert.Empty(parseResult.Errors);
        Assert.NotNull(parseResult.Result);

        return parseResult.Result;
    }
}