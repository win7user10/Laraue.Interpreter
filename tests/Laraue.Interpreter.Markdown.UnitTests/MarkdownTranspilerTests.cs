using System.Text.Json;
using Laraue.Interpreter.Markdown.UnitTests.Infrastructure;

namespace Laraue.Interpreter.Markdown.UnitTests;

public class MarkdownTranspilerTests
{
    private static readonly string NewLine = Environment.NewLine;
    
    [Fact]
    public void Transpile_ShouldReturnHeadersAndContent_WhenHeadersExist()
    {
        const string markdownFile = @"---
tags: [tag1, tag2]
project: project1
type: unitTestArticle
---

# The Quick Brown Fox

Welcome to the **Markdown to HTML Converter** by Laraue Software.

## Features

- *Live preview* as you type
- Toggle between **Rendered** view and **HTML source**
- One-click templates via the toolbar chips
- **Copy** or **Download** the generated HTML

## Code Example

```javascript
function greet(name) {
  return `Hello, ${name}!`;
}

console.log(greet(""world""));
```

## Blockquote

> ""Any fool can write code that a computer can understand.
> Good programmers write code that humans can understand.""
> — Martin Fowler

## Table

| Language | Paradigm    | Year |
|----------|-------------|------|
| C#       | OOP / FP    | 2000 |
| Python   | Multi       | 1991 |
| Rust     | Systems     | 2010 |

---

Made with ❤️ by [Laraue Software](https://laraue.com)";

        var result = new MarkdownTranspiler().ToHtml(markdownFile);
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
        
        Assert.Equal($"<h1>The Quick Brown Fox</h1><p>Welcome to the <b>Markdown to HTML Converter</b> by Laraue Software.</p><h2>Features</h2><ul><li><em>Live preview</em> as you type</li><li>Toggle between <b>Rendered</b> view and <b>HTML source</b></li><li>One-click templates via the toolbar chips</li><li><b>Copy</b> or <b>Download</b> the generated HTML</li></ul><h2>Code Example</h2><pre><code class=\"javascript\">function greet(name) {{{NewLine}  return `Hello, ${{name}}!`;{NewLine}}}{NewLine}{NewLine}console.log(greet(\"world\"));</code></pre><h2>Blockquote</h2><blockquote><p>\"Any fool can write code that a computer can understand.<br>Good programmers write code that humans can understand.\"<br>— Martin Fowler</p></blockquote><h2>Table</h2><table><thead><tr><th>Language</th><th>Paradigm</th><th>Year</th></tr></thead><tbody><tr><td>C#</td><td>OOP / FP</td><td>2000</td></tr><tr><td>Python</td><td>Multi</td><td>1991</td></tr><tr><td>Rust</td><td>Systems</td><td>2010</td></tr></tbody></table><p>---</p><p>Made with ❤️ by <a href=\"https://laraue.com\">Laraue Software</a></p>", result.HtmlContent);
    }

    [Fact]
    public void Transpile_ShouldReturnHeadersAndContent_Always()
    {
        var source =
            "{\"Content\":\"# The Quick Brown Fox\\n\\nWelcome to the **Markdown to HTML Converter** by Laraue Software.\\n\\n## Features\\n\\n- *Live preview* as you type\\n- Toggle between **Rendered** view and **HTML source**\\n- One-click templates via the toolbar chips\\n- **Copy** or **Download** the generated HTML\\n\\n## Code Example\\n\\n```javascript\\nfunction greet(name) {\\n  return `Hello, ${name}!`;\\n}\\n\\nconsole.log(greet(\\\"world\\\"));\\n```\\n\\n## Blockquote\\n\\n> \\\"Any fool can write code that a computer can understand.\\n> Good programmers write code that humans can understand.\\\"\\n> — Martin Fowler\\n\\n## Table\\n\\n| Language | Paradigm    | Year |\\n|----------|-------------|------|\\n| C#       | OOP / FP    | 2000 |\\n| Python   | Multi       | 1991 |\\n| Rust     | Systems     | 2010 |\\n\\n---\\n\\nMade with ❤️ by [Laraue Software](https://laraue.com)\\n\"}";

        var deserialized = JsonSerializer.Deserialize<SerializedObject>(source)!;
        
        var result = new MarkdownTranspiler().ToHtml(deserialized.Content);
        
        Assert.Equal($"<h1>The Quick Brown Fox</h1><p>Welcome to the <b>Markdown to HTML Converter</b> by Laraue Software.</p><h2>Features</h2><ul><li><em>Live preview</em> as you type</li><li>Toggle between <b>Rendered</b> view and <b>HTML source</b></li><li>One-click templates via the toolbar chips</li><li><b>Copy</b> or <b>Download</b> the generated HTML</li></ul><h2>Code Example</h2><pre><code class=\"javascript\">function greet(name) {{{NewLine}  return `Hello, ${{name}}!`;{NewLine}}}{NewLine}{NewLine}console.log(greet(\"world\"));</code></pre><h2>Blockquote</h2><blockquote><p>\"Any fool can write code that a computer can understand.<br>Good programmers write code that humans can understand.\"<br>— Martin Fowler</p></blockquote><h2>Table</h2><table><thead><tr><th>Language</th><th>Paradigm</th><th>Year</th></tr></thead><tbody><tr><td>C#</td><td>OOP / FP</td><td>2000</td></tr><tr><td>Python</td><td>Multi</td><td>1991</td></tr><tr><td>Rust</td><td>Systems</td><td>2010</td></tr></tbody></table><p>---</p><p>Made with ❤️ by <a href=\"https://laraue.com\">Laraue Software</a></p>", result.HtmlContent);
    }

    [Fact]
    public void Transpile_ShouldReturnContent_WhenHeadersNotExist()
    {
        const string markdownFile = "Only content";

        var result = new MarkdownTranspiler().ToHtml(markdownFile);
        
        Assert.Empty(result.Headers);
        Assert.Equal("<p>Only content</p>", result.HtmlContent);
    }

    public class SerializedObject
    {
        public required string Content { get; set; }
    }
}