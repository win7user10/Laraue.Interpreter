using Laraue.Interpreter.Markdown.UnitTests.Infrastructure;

namespace Laraue.Interpreter.Markdown.UnitTests;

public class MarkdownTranspilerTests
{
    [Fact]
    public void Transpile_ShouldReturnHeadersAndContent_WhenHeadersExist()
    {
        const string markdownFile = @"---
tags: [tag1, tag2]
project: project1
type: unitTestArticle
---

# Title of level 1

## Level 2 title
```csharp
var item = new Item();
```
Hi, _Italic_ __bold__ `font`  next string;
1. List 1 item 1
2. List 1 item 2
    3. List 1 subitem 1

- List 2 item 1
- List 2 item 2
    - List 2 subitem 1

| Name | Surname |
| ---  | ------- |
| Alex | Kent    |";

        var result = MarkdownTranspiler.ToHtml(markdownFile);
        var headers = result.Headers;
        
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
        
        // TODO - html tests
    }
    
    [Fact]
    public void Transpile_ShouldReturnContent_WhenHeadersNotExist()
    {
        const string markdownFile = "Only content";

        var result = MarkdownTranspiler.ToHtml(markdownFile);
        
        Assert.Empty(result.Headers);
        Assert.Equal("<p>Only content</p>", result.HtmlContent);
    }
}