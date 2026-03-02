using Laraue.Interpreter.Markdown.Body;
using Laraue.Interpreter.Markdown.Meta;
using Laraue.Interpreter.Parsing.Extensions;
using Laraue.Interpreter.Scanning.Extensions;

namespace Laraue.Interpreter.Markdown;

/// <summary>
/// The class contains methods to transpile Markdown. 
/// </summary>
public interface IMarkdownTranspiler
{
    /// <summary>
    /// Transpile Markdown to HTML.
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    public MarkdownTranspileResult ToHtml(string markdown);

    /// <summary>
    /// Get AST from the passed Markdown content. 
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    public MarkdownTreeResult GetTree(string markdown);
}

public class MarkdownTranspiler(
    WriteOptions options, 
    IMarkdownInnerLinksGenerator innerLinksGenerator)
    : IMarkdownTranspiler
{
    private readonly MarkdownTreeWriter _markdownTreeWriter = new(options);

    public MarkdownTranspiler() : this(
        new WriteOptions(),
        new MarkdownInnerLinksGenerator())
    {}
    
    /// <inheritdoc />
    public MarkdownTranspileResult ToHtml(string markdown)
    {
        var treeResult = GetTree(markdown);

        var content = _markdownTreeWriter.Write(treeResult.Tree);
        ICollection<MarkdownInnerLink> links = [];
        if (options.GenerateHeaderLinks)
            links = innerLinksGenerator.ParseLinks(treeResult.Tree);
        
        return new MarkdownTranspileResult
        {
            HtmlContent = content,
            Headers = treeResult.Headers,
            InnerLinks = links.ToArray(),
        };
    }
    
    /// <inheritdoc />
    public MarkdownTreeResult GetTree(string markdown)
    {
        var metaScanner = new MarkdownMetaTokenScanner(markdown);
        var metaScanResult = metaScanner.ScanTokens();
        metaScanResult.ThrowOnAnyError();
        
        var metaParser = new MarkdownMetaTokenParser(metaScanResult.Tokens);
        var metaParseResult = metaParser.Parse();
        metaParseResult.ThrowOnAnyError();
        
        var bodyScanner = new MarkdownTokenScanner(metaParseResult.Result!.Content);
        var bodyScanResult = bodyScanner.ScanTokens();
        bodyScanResult.ThrowOnAnyError();

        var bodyParser = new MarkdownTokenParser(bodyScanResult.Tokens);
        var bodyParseResult = bodyParser.Parse();
        bodyParseResult.ThrowOnAnyError();

        return new MarkdownTreeResult
        {
            Tree = bodyParseResult.Result!,
            Headers = metaParseResult.Result.Headers,
        };
    }
}
    
public class MarkdownTreeResult
{
    public required MarkdownTree Tree { get; init; }
    public required MarkdownHeader[] Headers { get; init; }
}