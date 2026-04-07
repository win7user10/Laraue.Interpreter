using Laraue.Interpreter.Markdown.Body;
using Laraue.Interpreter.Parsing.Extensions;
using Laraue.Interpreter.Scanning.Extensions;

namespace Laraue.Interpreter.Markdown.UnitTests;

public class MarkdownBodyParserTests
{
    [Fact]
    public void MarkdownWithoutSettings_ShouldBeRendered_Always()
    {
        var contentText = "hi";

        const string excepted = @"<p>
  hi
</p>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void Tables_ShouldBeRendered_Always()
    {
        var contentText = @"| Name | Age |
| --- | --- |
| Henry | 15 |
| Alex | 17 |";
        
        const string excepted = @"<table>
  <thead>
    <tr>
      <th>
        Name
      </th>
      <th>
        Age
      </th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>
        Henry
      </td>
      <td>
        15
      </td>
    </tr>
    <tr>
      <td>
        Alex
      </td>
      <td>
        17
      </td>
    </tr>
  </tbody>
</table>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void Tables_ShouldBeRendered_WhenHeadersMissing()
    {
        var contentText = @"|                   |                   |
|-------------------|-------------------|
| Cell 1 | Cell 2 | ";
        
        const string excepted = @"<table>
  <thead>
    <tr>
      <th></th>
      <th></th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>
        Cell 1
      </td>
      <td>
        Cell 2
      </td>
    </tr>
  </tbody>
</table>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void Tables_ShouldBeRenderedWithInlineItems_Always()
    {
        var contentText = @"| Name | Link      |
| -- | -------- |
| John | No link |
| Henry | ![mountain](mountain.jpg) |";

        const string excepted = @"<table>
  <thead>
    <tr>
      <th>
        Name
      </th>
      <th>
        Link
      </th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>
        John
      </td>
      <td>
        No link
      </td>
    </tr>
    <tr>
      <td>
        Henry
      </td>
      <td>
        <img src=""mountain.jpg"" alt=""mountain"" />
      </td>
    </tr>
  </tbody>
</table>";
        
        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void OrderedLists_ShouldBeRendered_WhenStructureIsHierarchical()
    {
        var contentText = @"1. Item #1
1. Item #2
    1. Item #3";
        
        const string excepted = @"<ol>
  <li>
    Item #1
  </li>
  <li>
    Item #2
  </li>
  <ol>
    <li>
      Item #3
    </li>
  </ol>
</ol>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void OrderedLists_ShouldBeRendered_WhenContentIsMixed()
    {
        var contentText = @"1. Item #1
Description [link](http://test_1.com)
1. Item #2

## [Heading](https://test.com)
And text";
        
        const string excepted = @"<ol>
  <li>
    Item #1 Description <a href=""http://test_1.com"">link</a>
  </li>
  <li>
    Item #2
  </li>
</ol>
<h2 id=""heading"">
  <a href=""https://test.com"">Heading</a>
</h2>
<p>
  And text
</p>";

        Assert.Equal(excepted, ToHtml(contentText, generateHeaderLinks: true));
    }
    
    [Fact]
    public void UnorderedLists_ShouldBeRendered_WhenStructureIsHierarchical()
    {
        var contentText = @"- Item #1
- Item #2  
Hi
    - Item #3
        - Item #4";
        
        const string excepted = @"<ul>
  <li>
    Item #1
  </li>
  <li>
    Item #2
    Hi
  </li>
  <ul>
    <li>
      Item #3
    </li>
    <ul>
      <li>
        Item #4
      </li>
    </ul>
  </ul>
</ul>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Theory]
    [InlineData("Hi, _Ann_")]
    [InlineData("Hi, *Ann*")]
    public void ItalicItems_ShouldBeRendered_Always(string text)
    {
        const string excepted = @"<p>
  Hi, <em>
    Ann
  </em>
</p>";
      
        Assert.Equal(excepted, ToHtml(text));
    }
    
    [Theory]
    [InlineData("Hi, __Ann__")]
    [InlineData("Hi, **Ann**")]
    public void BoldItems_ShouldBeRendered_Always(string text)
    {
        const string excepted = @"<p>
  Hi, <b>
    Ann
  </b>
</p>";
      
        Assert.Equal(excepted, ToHtml(text));
    }
    
    [Fact]
    public void BoldItems_ShouldBeRendered_WhenInsideLists()
    {
        var contentText = @"List title
1. **First:** item
2. **Second:** item";
        
        const string excepted = @"<p>
  List title
</p>
<ol>
  <li>
    <b>
      First:
    </b> item
  </li>
  <li>
    <b>
      Second:
    </b> item
  </li>
</ol>";
        
        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void Carries_ShouldBeAdded_AfterTwoWhitespaces()
    {
        var contentText = @"**Hey, guys**  
I am here";
        
        const string excepted = @"<p>
  <b>
    Hey, guys
  </b>
</p>
<p>
  I am here
</p>";
        
        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void Carries_ShouldBeAdded_AfterTwoNewLines()
    {
        var contentText = @"**Hey, guys**

I am here";
        
        const string excepted = @"<p>
  <b>
    Hey, guys
  </b>
</p>
<p>
  I am here
</p>";
        
        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void Carries_ShouldBeRendered_Always()
    {
        var contentText = @"Hello guys,
and girls";
        
        const string excepted = @"<p>
  Hello guys, and girls
</p>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void InlineCode_ShouldBeRendered_Always()
    {
        var contentText = "This is `string[] Tags` inline code";
        
        const string excepted = @"<p>
  This is <code>
    string[] Tags
  </code> inline code
</p>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void CodeBlock_ShouldBeRendered_Always()
    {
        var contentText = @"```csharp
var user = new User<Guid>();
var html = ""<html><p class=""title"">Hi</html>""
```";
        
        const string excepted = @"<pre>
  <code class=""csharp"">var user = new User&lt;Guid&gt;();
var html = ""&lt;html&gt;&lt;p class=""title""&gt;Hi&lt;/html&gt;""</code>
</pre>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    
    [Fact]
    public void NestedCodeBlocks_ShouldKeepFormatting_Always()
    {
        var contentText = @"```json
[
 [
  ""Cell1"",
  ""Cell2""
 ]
]
```";
        
        const string excepted = @"<pre>
  <code class=""json"">[
 [
  ""Cell1"",
  ""Cell2""
 ]
]</code>
</pre>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void Link_ShouldBeRendered_Always()
    {
        var contentText = @"The [link](https://google.com/page1.html)
inside text"; 
        
        const string excepted = @"<p>
  The <a href=""https://google.com/page1.html"">link</a> inside text
</p>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void Image_ShouldBeRendered_Always()
    {
        var contentText = @"![Big mountain](/assets/mountain.jpg ""Everest"")
![Small mountain](/assets/mini-mountain.jpg ""Elbrus"")";
        
        const string excepted = @"<p>
  <img src=""/assets/mountain.jpg"" title=""Everest"" alt=""Big mountain"" /> <img src=""/assets/mini-mountain.jpg"" title=""Elbrus"" alt=""Small mountain"" />
</p>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void ImageWithLink_ShouldBeRendered_Always()
    {
        var contentText = @"[![Big mountain](/assets/mountain.jpg ""Everest"")](http://link)";
        
        const string excepted = @"<p>
  <a href=""http://link""><img src=""/assets/mountain.jpg"" title=""Everest"" alt=""Big mountain"" /></a>
</p>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void BlocksAfterInlineElement_ShouldBeRendered_Always()
    {
        var contentText = @"![mountain](mountain.jpg)
## Next title";

        const string excepted = @"<p>
  <img src=""mountain.jpg"" alt=""mountain"" />
</p>
<h2>
  Next title
</h2>";

        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void Blockquotes_ShouldBeRendered_Always()
    {
        var contentText = @"## Blockquote

> ""Quote Line 1
> Quote Line 2""";
        
        const string excepted = @"<h2>
  Blockquote
</h2>
<blockquote>
  <p>
    ""Quote Line 1<br>Quote Line 2""
  </p>
</blockquote>";
        
        Assert.Equal(excepted, ToHtml(contentText));
    }
    
    [Fact]
    public void HrBlocks_ShouldBeRendered_Always()
    {
        var contentText = @"Hey
---
Next line";
        
        const string excepted = @"<p>
  Hey
</p>
<hr>
<p>
  Next line
</p>";
        
        Assert.Equal(excepted, ToHtml(contentText));
    }

    private static string ToHtml(string markdown, bool generateHeaderLinks = false)
    {
        var scanner = new MarkdownTokenScanner(markdown);
        var scanResult = scanner.ScanTokens();
        scanResult.ThrowOnAnyError();

        var parser = new MarkdownTokenParser(scanResult.Tokens);
        var parseResult = parser.Parse();
        parseResult.ThrowOnAnyError();

        var result = new MarkdownTreeWriter(
          new WriteOptions
          {
            GenerateHeaderLinks = generateHeaderLinks
          })
          .Write(parseResult.Result!);

        return result.Replace("\r\n", Environment.NewLine);
    }
}