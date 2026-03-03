using System.Text;
using Laraue.Interpreter.Parsing;
using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Markdown.Meta;

public class MarkdownMetaTokenParser(Token<MarkdownMetaTokenType>[] tokens)
    : TokenParser<MarkdownMetaTokenType, MarkdownMetaTree>(tokens)
{
    protected override MarkdownMetaTree ParseInternal()
    {
        var headers = Array.Empty<MarkdownHeader>();
        
        Skip(MarkdownMetaTokenType.NewLine); // Empty lines are allowed before meta section
        
        if (Match(MarkdownMetaTokenType.MetaDelimiter)) // Headers can not exist
            headers = ReadHeaders();
        
        Skip(MarkdownMetaTokenType.NewLine); // Empty lines are allowed after meta section
        
        var content = string.Empty;
        if (Match(MarkdownMetaTokenType.Content))
            content = Previous().Lexeme!;
        
        return new MarkdownMetaTree
        {
            Headers = headers,
            Content = content
        };
    }

    private MarkdownHeader[] ReadHeaders()
    {
        var headers = new List<MarkdownHeader>();

        // Read all header rows
        while (!IsParseCompleted)
        {
            // Empty lines does not matter here
            Skip(MarkdownMetaTokenType.NewLine);
            
            // Until meta delimiter is not met
            if (Match(MarkdownMetaTokenType.MetaDelimiter))
                return headers.ToArray();
            
            headers.Add(ReadHeader());
        }
        
        // If meta delimiter is not met, throw
        throw Error(Peek(), "Meta section closing was excepted");
    }
    
    private MarkdownHeader ReadHeader()
    {
        var word = Consume(
            MarkdownMetaTokenType.Word,
            "Meta property name is excepted");
        
        Consume(
            MarkdownMetaTokenType.Delimiter,
            "':' excepted after meta property definition");
        
        Skip(MarkdownMetaTokenType.WhiteSpace);

        var value = ConsumeValue();

        return new MarkdownHeader
        {
            PropertyName = (word.Literal as string)!,
            Value = value,
            LineNumber = Previous().LineNumber,
        };
    }

    private object ConsumeValue()
    {
        var startToken = CurrentIndex;
        if (!Match(MarkdownMetaTokenType.ArrayStart))
            return ConsumeSingleValue();
        
        var arrayResult = new List<string>();
        var singleResult = new StringBuilder();

        while (!IsParseCompleted && !Match(MarkdownMetaTokenType.NewLine))
        {
            if (Match(MarkdownMetaTokenType.Comma))
            {
                arrayResult.Add(singleResult.ToString());
                singleResult.Clear();
                continue;
            }

            if (Match(MarkdownMetaTokenType.ArrayEnd))
            {
                arrayResult.Add(singleResult.ToString());
                return arrayResult.ToArray();
            }
            
            var next = Advance();
            singleResult.Append(next.Literal as string);
        }

        var sb = new StringBuilder();
        var last = Previous();
        for (var i = startToken;; i++)
        {
            var next = Take(i);
            if (next == last)
                break;
            sb.Append(next.Literal ?? next.Lexeme);
        }
        
        return sb.ToString();
    }

    private string ConsumeSingleValue()
    {
        var singleResult = new StringBuilder();

        while (!IsParseCompleted && !Check(MarkdownMetaTokenType.NewLine))
        {
            var next = Advance();
            singleResult.Append(next.Literal ?? next.Lexeme);
        }
            
        return singleResult.ToString();
    }
}