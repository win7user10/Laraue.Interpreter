using Laraue.Interpreter.Markdown.Body;
using Laraue.Interpreter.Markdown.Meta;
using Laraue.Interpreter.Parsing.Extensions;
using Laraue.Interpreter.Scanning.Extensions;

namespace Laraue.Interpreter.Markdown;

/// <summary>
/// The class contains methods to transpile Markdown. 
/// </summary>
public static class MarkdownTranspiler
{
    /// <summary>
    /// Transpile Markdown to HTML.
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    public static MarkdownTranspileResult ToHtml(string markdown)
    {
        var treeResult = GetTree(markdown);

        var content = new MarkdownTreeWriter().Write(treeResult.Tree);
        return new MarkdownTranspileResult
        {
            HtmlContent = content,
            Headers = treeResult.Headers,
        };
    }
    
    /// <summary>
    /// Get AST from the passed Markdown content. 
    /// </summary>
    /// <param name="markdown"></param>
    /// <returns></returns>
    public static MarkdownTreeResult GetTree(string markdown)
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
    
    public class MarkdownTreeResult
    {
        public required MarkdownTree Tree { get; init; }
        public required MarkdownHeader[] Headers { get; init; }
    }
}