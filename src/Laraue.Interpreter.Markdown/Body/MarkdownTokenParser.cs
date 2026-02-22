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
        if (MatchSequential(MarkdownTokenType.Backtick, 3))
            return ReadCode();

        throw new NotImplementedException(Peek().TokenType.ToString());
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
        var result = new List<MarkdownContentBlockElement>();

        while (true)
        {
            result.AddRange(ReadRowElements());
            
            // Skip new line
            Advance();
            
            // If after new line the word is appearing, it is the same paragraph
            if (!CheckSkipping(MarkdownTokenType.Word, MarkdownTokenType.Whitespace))
                break;
            
            result.Add(new PlainMarkdownContentBlockElement
            {
                Content = " "
            });
        }

        return new PlainMarkdownContentBlock
        {
            Elements = result.ToArray()
        };
    }
    
    private CodeMarkdownContentBlock ReadCode()
    {
        var result = new List<MarkdownContentBlockElement>();
        
        string? language = null;
        if (Match(MarkdownTokenType.Word))
            language = Previous().Literal?.ToString();
        
        // Whitespaces after the block declaration doesn't matter
        Skip(MarkdownTokenType.NewLine);
        Skip(MarkdownTokenType.Whitespace);

        var lastNonEmptyTokenNumber = 0;
        
        while (!IsParseCompleted && !MatchSequential(MarkdownTokenType.Backtick, 3))
        {
            var consumed = Peek();
            
            var element = ReadElement();
            result.Add(element);

            if (consumed.TokenType is not MarkdownTokenType.NewLine and not MarkdownTokenType.Whitespace)
                lastNonEmptyTokenNumber = result.Count;
        }

        return new CodeMarkdownContentBlock
        {
            Elements = result.Take(lastNonEmptyTokenNumber).ToArray(),
            Language = language
        };
    }

    private MarkdownContentBlockElement[] ReadRowElements()
    {
        var result = new List<MarkdownContentBlockElement>();
        
        // Whitespaces while reading row doesn't matter.
        Skip(MarkdownTokenType.Whitespace);
        
        var lastNonEmptyTokenNumber = 0;
        while (!IsParseCompleted && !Check(MarkdownTokenType.NewLine))
        {
            var consumed = Peek();
            
            var element = ReadElement();
            result.Add(element);
            
            if (consumed.TokenType is not MarkdownTokenType.NewLine and not MarkdownTokenType.Whitespace)
                lastNonEmptyTokenNumber = result.Count;
        }
        
        return result.Take(lastNonEmptyTokenNumber).ToArray();
    }
    
    private MarkdownContentBlockElement ReadElement()
    {
        if (Match(MarkdownTokenType.Asterisk))
            return ReadItalicOrBoldElement(MarkdownTokenType.Asterisk);
        
        if (Match(MarkdownTokenType.Underscore))
            return ReadItalicOrBoldElement(MarkdownTokenType.Underscore);
        
        if (Match(MarkdownTokenType.Backtick))
            return ReadBacktickElement();

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
    
    private MarkdownContentBlockElement ReadItalicOrBoldElement(MarkdownTokenType tokenType)
    {
        // If the element is written twice - it is the "bold" case
        if (Match(tokenType))
            return ReadBoldElement(tokenType);
        
        // Otherwise - it is "italic" case
        var elements = new List<MarkdownContentBlockElement>();
        while (!IsRowEndReached() && !Match(tokenType))
        {
            var next = ReadElement();
            elements.Add(next);
        }

        return new ItalicMarkdownContentBlockElement
        {
            InnerElements = elements.ToArray()
        };
    }

    private BoldMarkdownContentBlockElement ReadBoldElement(
        MarkdownTokenType tokenType)
    {
        var elements = new List<MarkdownContentBlockElement>();
        while (!IsRowEndReached())
        {
            if (CheckSequential(tokenType, tokenType))
            {
                Advance(2);
                break;
            }
            
            var next = ReadElement();
            elements.Add(next);
        }

        return new BoldMarkdownContentBlockElement
        {
            InnerElements = elements.ToArray()
        };
    }
    
    private InlineCodeMarkdownContentBlockElement ReadBacktickElement()
    {
        var elements = new List<MarkdownContentBlockElement>();
        while (!IsRowEndReached() && !Match(MarkdownTokenType.Backtick))
        {
            var next = ReadPlainElement();
            elements.Add(next);
        }

        return new InlineCodeMarkdownContentBlockElement
        {
            InnerElements = elements.ToArray()
        };
    }

    private bool IsRowEndReached()
    {
        return IsParseCompleted || Check(MarkdownTokenType.NewLine);
    }
}