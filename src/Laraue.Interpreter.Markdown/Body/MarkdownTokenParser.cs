using System.Text;
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
            ContentBlocks = PostProcessBlocks(contentBlocks),
        };
    }

    private static MarkdownContentBlock[] PostProcessBlocks(
        List<MarkdownContentBlock> blocks)
    {
        // Merge the parts of md file in one line
        var result = new List<MarkdownContentBlock>();
        
        MarkdownContentBlock? previous = null;
        var blocksIterator = blocks.GetEnumerator();
        
        while (blocksIterator.MoveNext())
        {
            if (previous is not PlainMarkdownContentBlock plainMarkdownContentBlock)
            {
                previous = blocksIterator.Current;
                result.Add(previous);
                continue;
            }

            var mergedElements = new List<MarkdownContentBlockElement>(plainMarkdownContentBlock.Elements);
            
            do
            {
                var current = blocksIterator.Current;
                if (current is PlainMarkdownContentBlock currentPlainBlock)
                {
                    mergedElements.Add(new PlainMarkdownContentBlockElement
                    {
                        Content = " "
                    });
                    
                    mergedElements.AddRange(currentPlainBlock.Elements);
                    if (!blocksIterator.MoveNext())
                        break;
                }
                else
                    break;

            } while (true);

            if (mergedElements.Count > 0)
            {
                plainMarkdownContentBlock.Elements = mergedElements.ToArray();
            }
        }

        return result.ToArray();
    }

    private MarkdownContentBlock ReadNextBlock()
    {
        if (Check(MarkdownTokenType.NumberSign))
            return ReadHeading();
        if (MatchSequential(MarkdownTokenType.Backtick, 3))
            return ReadCode();
        return ReadPlain();
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
        return new PlainMarkdownContentBlock
        {
            Elements = ReadRowElements(),
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
        
        if (Match(MarkdownTokenType.LeftSquareBracket))
            return ReadLink();
        
        if (Match(MarkdownTokenType.Not))
            return ReadImage();

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
    
    private LinkCodeMarkdownContentBlockElement ReadLink()
    {
        var linkContentBuilder = new StringBuilder();
        while (!IsRowEndReached() && !Match(MarkdownTokenType.RightSquareBracket))
        {
            var next = ReadPlainElement();
            linkContentBuilder.Append(next.Content);
        }

        string? href = null;
        if (Match(MarkdownTokenType.LeftParenthesis))
        {
            var hrefBuilder = new StringBuilder();
            while (!IsRowEndReached() && !Match(MarkdownTokenType.RightParenthesis))
            {
                var next = ReadPlainElement();
                hrefBuilder.Append(next.Content);
            }
            
            href = hrefBuilder.ToString();
        }

        return new LinkCodeMarkdownContentBlockElement
        {
            Title = linkContentBuilder.ToString(),
            Href = href,
        };
    }
    
    private MarkdownContentBlockElement ReadImage()
    {
        var altBuilder = new StringBuilder();
        if (Match(MarkdownTokenType.LeftSquareBracket))
        {
            while (!IsRowEndReached() && !Match(MarkdownTokenType.RightSquareBracket))
            {
                var next = ReadPlainElement();
                altBuilder.Append(next.Content);
            }
        }
        
        Skip(MarkdownTokenType.Whitespace);

        var srcBuilder = new StringBuilder();
        var titleBuilder = new StringBuilder();
        
        if (Match(MarkdownTokenType.LeftParenthesis))
        {
            // Start reading the src
            while (!IsRowEndReached())
            {
                Skip(MarkdownTokenType.Whitespace);

                // The link definition is finished
                if (Match(MarkdownTokenType.RightParenthesis))
                    break;

                // Title definition started
                if (Match(MarkdownTokenType.Quote))
                {
                    while (!IsRowEndReached() && !Match(MarkdownTokenType.Quote))
                    {
                        var titlePart = ReadPlainElement();
                        titleBuilder.Append(titlePart.Content);
                    }
                    
                    continue;
                }
                
                var srcPart = ReadPlainElement();
                srcBuilder.Append(srcPart.Content);
            }
        }

        return new ImageCodeMarkdownContentBlockElement
        {
            Title = titleBuilder.Length > 0 ? titleBuilder.ToString() : null,
            Src = srcBuilder.Length > 0 ? srcBuilder.ToString() : null,
            Alt = altBuilder.Length > 0 ? altBuilder.ToString() : null,
        };
    }

    private bool IsRowEndReached()
    {
        return IsParseCompleted || Check(MarkdownTokenType.NewLine);
    }
}