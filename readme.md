# Laraue.Interpreter  

The project is the just set of tools to implement a new Interpreter in CSharp.  
Based on the examples from the [Crafting interpreters book](https://craftinginterpreters.com)  

| Package                      | Link                                                                                                                                          | Downloads                                                                                                                                       |
|-------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| Laraue.Interpreter.Scanning  | [![latest version](https://img.shields.io/nuget/v/Laraue.Interpreter.Scanning)](https://www.nuget.org/packages/Laraue.Interpreter.Scanning)  | [![latest version](https://img.shields.io/nuget/dt/Laraue.Interpreter.Scanning)](https://www.nuget.org/packages/Laraue.Interpreter.Scanning)  |
| Laraue.Interpreter.Parsing   | [![latest version](https://img.shields.io/nuget/v/Laraue.Interpreter.Parsing)](https://www.nuget.org/packages/Laraue.Interpreter.Parsing)    | [![latest version](https://img.shields.io/nuget/dt/Laraue.Interpreter.Parsing)](https://www.nuget.org/packages/Laraue.Interpreter.Parsing)    |
| Laraue.Interpreter.Common    | [![latest version](https://img.shields.io/nuget/v/Laraue.Interpreter.Common)](https://www.nuget.org/packages/Laraue.Interpreter.Common)      | [![latest version](https://img.shields.io/nuget/dt/Laraue.Interpreter.Common)](https://www.nuget.org/packages/Laraue.Interpreter.Common)      |
| Laraue.Interpreter.Markdown  | [![latest version](https://img.shields.io/nuget/v/Laraue.Interpreter.Markdown)](https://www.nuget.org/packages/Laraue.Interpreter.Markdown)  | [![latest version](https://img.shields.io/nuget/dt/Laraue.Interpreter.Markdown)](https://www.nuget.org/packages/Laraue.Interpreter.Markdown)  |

## Laraue.Interpreter.Scanning

Contains the scanner base class. Create the token enum

```csharp
public enum MdTokenType
{
    /// <summary>
    /// '*'
    /// </summary>
    Asterisk,
    
    /// <summary>
    /// '**'
    /// </summary>
    DoubleAsterisk,
    
    /// <summary>
    /// '`'
    /// </summary>
    Backtick,
    
    ...
}
```

and implement the scanner

```csharp
public class MdTokenScanner(string input)
{
    protected override bool TryProcessNextChar(char nextChar)
    {
        switch (nextChar)
        {
            case '*':
                AddToken(PopNextCharIf(c => c == '*') ? MdTokenType.DoubleAsterisk : MdTokenType.Asterisk);
                return true;
            case '`':
                AddToken(MdTokenType.Backtick);
                return true;
            
            ...
        }
}
```

now you can get the token scan result for the string
```csharp
var scanner = new MdTokenScanner(markdownFile);
var result = scanner.ScanTokens();
```

## Laraue.Interpreter.Parsing

Contains the parser base class. Create the parsing result class
```csharp
public class MdTokenExpr
{
    public required MdHeader[] Headers { get; set; }
    public required ContentBlock[] Content { get; set; }
}
```

And implement it like this

```csharp
public class MdTokenParser : TokenParser<MdTokenType, MdTokenExpr>
{
    protected override MdTokenExpr ParseInternal()
    {
        MdHeader[] headers = [];
        var contentBlocks = new List<ContentBlock>();
        
        Skip(MdTokenType.NewLine);
        if (CheckSequential(MdTokenType.MinusSign, 3))
        {
            headers = ConsumeHeaders(); // Consume headers implementation
        }
        
        while (!IsParseCompleted)
        {
            contentBlocks.Add(ReadNewLineBlock()); // Implement md raw reading
        }

        return new MdTokenExpr
        {
            Headers = headers,
            Content = contentBlocks.ToArray(),
        };
    }
}
```

now the token sequence from the scanner can be parsed
```csharp
var mdTokenParser = new MdTokenParser(result.Tokens);
var parseResult = mdTokenParser.Parse();
```

## Laraue.Interpreter.Markdown

A Markdown-to-HTML transpiler built on top of the scanning/parsing packages above.

```csharp
var transpiler = new MarkdownTranspiler();
var result = transpiler.ToHtml(markdownText);

Console.WriteLine(result.HtmlContent);
```

`ToHtml` also returns any YAML-like front matter headers (`result.Headers`) and, when
`WriteOptions.GenerateHeaderLinks` is enabled, a table of contents built from the document's
headings (`result.InnerLinks`).

### Supported syntax

| Syntax                                     | Example                                  | Renders as                                  |
|---------------------------------------------|-------------------------------------------|-----------------------------------------------|
| Headings                                    | `# Title` … `###### Title`                | `<h1>` … `<h6>`                               |
| Bold                                        | `**text**` or `__text__`                  | `<b>`                                          |
| Italic                                      | `*text*` or `_text_`                      | `<em>`                                         |
| Strikethrough                               | `~~text~~`                                | `<del>`                                        |
| Nested emphasis                             | `**bold *and italic* text**`              | `<b>bold <em>and italic</em> text</b>`         |
| Inline code                                 | `` `code` ``                              | `<code>`                                       |
| Fenced code block                           | ```` ```csharp ... ``` ````               | `<pre><code class="csharp">`                   |
| Links                                       | `[text](href)`                            | `<a href="href">`                              |
| Images                                      | `![alt](src "title")`                     | `<img src alt title />`                        |
| Blockquote                                  | `> quoted text`                           | `<blockquote>`                                 |
| Horizontal rule                             | `---`                                     | `<hr>`                                         |
| Ordered / unordered lists                   | `1. item` / `- item` (nestable, mixable)  | `<ol>` / `<ul>`                                |
| Tables, with optional column alignment      | `\| a \| b \|` + `\| :--- \| ---: \|`     | `<table>` with `style="text-align: ..."`       |
| Escaped characters                          | `\*not italic\*`                          | literal `*not italic*`                         |
| YAML-like front matter                      | `--- title: Post --- ...`                 | exposed as `result.Headers`, not rendered      |

Unmatched delimiters (a stray `*`, `_`, `[`, or `!` with no closing counterpart on the same line)
are rendered as literal text instead of silently swallowing the rest of the line.

### Extending the syntax

The parser and writer accept a list of `IMarkdownExtension` implementations, so new inline
syntax can be added without touching the core scanner/parser/writer. An extension implements:

- `CanRead` / `Read` (`IMarkdownInlineExtension`) — recognize and parse the custom syntax using
  the `IMarkdownInlineParserContext` cursor (peek/advance tokens, read raw text up to a
  delimiter, recurse into nested inline content).
- `CanWrite` / `Write` (`IMarkdownElementWriterExtension`) — render the resulting AST element
  using the `IMarkdownWriterContext` (append text, indent, write nested child elements).

For example, `VideoMarkdownExtension` (ships in `Laraue.Interpreter.Markdown.Body.Extensibility.Examples`)
adds a `!video[alt](src)` syntax rendering to an HTML5 `<video>` tag:

```csharp
var transpiler = new MarkdownTranspiler(
    new WriteOptions(),
    new MarkdownInnerLinksGenerator(),
    extensions: [new VideoMarkdownExtension()]);

transpiler.ToHtml("!video[My clip](movie.mp4)");
// <p>
//   <video controls src="movie.mp4">My clip</video>
// </p>
```

Without registering an extension, its syntax simply falls through to the normal parsing rules
(e.g. unregistered `!video[...]` parses as plain text followed by a link), so extensions are
strictly opt-in.