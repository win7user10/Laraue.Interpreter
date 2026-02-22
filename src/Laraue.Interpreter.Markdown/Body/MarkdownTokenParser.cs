using Laraue.Interpreter.Parsing;
using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Markdown.Body;

public class MarkdownTokenParser(Token<MarkdownTokenType>[] tokens)
    : TokenParser<MarkdownTokenType, MarkdownTree>(tokens)
{
    protected override MarkdownTree ParseInternal()
    {
        var contentBlocks = new List<MarkdownContentBlock>();

        while (!IsParseCompleted)
        {
            if (Match(MarkdownTokenType.NewLine))
                continue;
            
            if (Match(MarkdownTokenType.Whitespace))
                continue;
            
            contentBlocks.Add(ReadNextBlock());
        }

        return new MarkdownTree
        {
            ContentBlocks = contentBlocks.ToArray(),
        };
    }

    private MarkdownContentBlock ReadNextBlock()
    {
        if (Check(MarkdownTokenType.NumberSign))
            return ReadHeading();
        if (Check(MarkdownTokenType.Word))
            return ReadPlain();

        throw Error(Peek(), "Unexpected token.");
    }

    private HeadingMarkdownContentBlock ReadHeading()
    {
        var headingLevel = 0;
        while (Match(MarkdownTokenType.NumberSign))
            headingLevel++;

        var elements = ReadRowElements();
        return new HeadingMarkdownContentBlock
        {
            Elements = elements,
            Level = headingLevel
        };
    }
    
    private PlainMarkdownContentBlock ReadPlain()
    {
        var elements = ReadRowElements();
        return new PlainMarkdownContentBlock
        {
            Elements = elements,
        };
    }

    private MarkdownContentBlockElement[] ReadRowElements()
    {
        var result = new List<MarkdownContentBlockElement>();
        
        // Whitespaces while reading row doesn't matter.
        Skip(MarkdownTokenType.Whitespace);
        
        while (!IsParseCompleted && !Check(MarkdownTokenType.NewLine))
        {
            var element = ReadElement();
            result.Add(element);
        }
        
        return result.ToArray();
    }
    
    private MarkdownContentBlockElement ReadElement()
    {
        if (Match(MarkdownTokenType.Asterisk))
            return ReadAsteriskElement();

        return ReadPlainElement();
    }

    private PlainMarkdownContentBlockElement ReadPlainElement()
    {
        var element = Advance();

        return new PlainMarkdownContentBlockElement
        {
            Content = element.Literal?.ToString() ?? element.Lexeme!,
        };
    }
    
    private BoldMarkdownContentBlockElement ReadAsteriskElement()
    {
        throw new NotImplementedException();
    }
}