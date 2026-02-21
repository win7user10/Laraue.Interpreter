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

        var content = Consume(
            MarkdownMetaTokenType.Content,
            "Content block is excepted");
        
        return new MarkdownMetaTree
        {
            Headers = headers,
            Content = content.Lexeme!
        };
    }

    private MarkdownHeader[] ReadHeaders()
    {
        var headers = new List<MarkdownHeader>();

        // Read all header rows
        while (Match(MarkdownMetaTokenType.NewLine))
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

        var value = ConsumeValue();

        return new MarkdownHeader
        {
            PropertyName = (word.Literal as string)!,
            Value = value,
            LineNumber = Peek().LineNumber,
        };
    }

    private object ConsumeValue()
    {
        var result = new List<object>();

        if (!Match(MarkdownMetaTokenType.ArrayStart))
            return ConsumeSingleValue();
        
        do
        {
            var nextValue = ConsumeSingleValue();
            result.Add(nextValue);
        } while (Match(MarkdownMetaTokenType.Comma));
            
        Consume(MarkdownMetaTokenType.ArrayEnd, "']' excepted");
        return result.ToArray();
    }
    
    private string ConsumeSingleValue()
    {
        var word = Consume(
            MarkdownMetaTokenType.Word,
            "The property definition excepted");

        return (word.Literal as string)!;
    }
}