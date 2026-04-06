using System.Diagnostics.CodeAnalysis;
using System.Text;
using Laraue.Interpreter.Markdown.Body.BlockElements;
using Laraue.Interpreter.Markdown.Body.Blocks;
using Laraue.Interpreter.Parsing;
using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Markdown.Body;

public class MarkdownTokenParser
    : TokenParser<MarkdownTokenType, MarkdownTree>
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

    private class ReadBlockDelegate
    {
        public required Func<bool> IsApplicable { get; init; }
        public required Func<MarkdownContentBlock> Read { get; init; }
    }

    private readonly List<ReadBlockDelegate> _readBlockDelegates = new ();

    public MarkdownTokenParser(Token<MarkdownTokenType>[] tokens) : base(tokens)
    {
        _readBlockDelegates.AddRange(
            new ReadBlockDelegate
            {
                IsApplicable = () => Check(MarkdownTokenType.NumberSign),
                Read = ReadHeading
            },
            new ReadBlockDelegate
            {
                IsApplicable = () => Check(MarkdownTokenType.Pipe),
                Read = ReadTable
            },
            new ReadBlockDelegate
            {
                IsApplicable = () => CheckSequential(MarkdownTokenType.Backtick, 3),
                Read = ReadCode
            },
            new ReadBlockDelegate
            {
                IsApplicable = () => Check(MarkdownTokenType.GreaterThan),
                Read = ReadQuote
            },
            new ReadBlockDelegate
            {
                IsApplicable = () => CheckSequential(
                    MarkdownTokenType.Number,
                    MarkdownTokenType.Dot,
                    MarkdownTokenType.Whitespace),
                Read = ReadOrderedList
            },
            new ReadBlockDelegate
            {
                IsApplicable = () => CheckSequential(
                    MarkdownTokenType.MinusSign,
                    MarkdownTokenType.Whitespace),
                Read = ReadUnorderedList
            }
        );
    }

    private MarkdownContentBlock ReadNextBlock()
    {
        var readDelegate = _readBlockDelegates
            .FirstOrDefault(x => x.IsApplicable());

        return readDelegate is not null ? readDelegate.Read() : ReadPlain();
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
    
    private TableContentBlock ReadTable()
    {
        var rows = new List<TableContentBlockRow>();
        while (TryReadTableRow(out var row))
            rows.Add(row);

        if (IsTableContentDivider(rows.First()))
        {
            return new TableContentBlock
            {
                Header = null,
                Rows = rows.Skip(1).ToArray()
            };
        }
        
        return new TableContentBlock
        {
            Header = rows.First(),
            Rows = rows.Skip(2).ToArray()
        };
    }
    
    private BlockquoteContentBlock ReadQuote()
    {
        var rows = new List<MarkdownContentBlockElement[]>();
        while (!IsParseCompleted && Match(MarkdownTokenType.GreaterThan))
        {
            var elements = ReadRowElements();
            rows.Add(elements);
        }

        return new BlockquoteContentBlock
        {
            Elements = rows
        };
    }

    private bool IsTableContentDivider(TableContentBlockRow row)
    {
        return row.Cells.Length > 0 && row.Cells
            .All(c => c.Elements.Length > 0 && c.Elements
                .All(e => e is PlainMarkdownContentBlockElement { Content: "-" }));
    }

    private bool TryReadTableRow(
        [NotNullWhen(true)] out TableContentBlockRow? row)
    {
        row = null;
        if (!Match(MarkdownTokenType.Pipe))
            return false;
        
        var rowItems = new List<List<MarkdownContentBlockElement>>();
        
        var nextCellElements = new List<MarkdownContentBlockElement>();
        while (!IsParseCompleted && !Match(MarkdownTokenType.NewLine))
        {
            if (Match(MarkdownTokenType.Pipe))
            {
                rowItems.Add(nextCellElements);
                nextCellElements = [];
                continue;
            }
            
            var nextCellElement = ReadElement();
            nextCellElements.Add(nextCellElement);
        }

        var cells = rowItems
            .Select(h => new TableContentBlockCell
            {
                Elements = h.Trim(" ").ToArray()
            })
            .ToArray();

        row = new TableContentBlockRow
        {
            Cells = cells
        };

        return true;
    }
    
    private PlainMarkdownContentBlock ReadPlain()
    {
        var result = new List<MarkdownContentBlockElement>();
        while (!IsParseCompleted)
        {
            result.AddRange(ReadRowElements());
            
            // Unite some blocks paragraph block into the one 
            Skip(MarkdownTokenType.Whitespace, MarkdownTokenType.NewLine);
            if (_readBlockDelegates.Any(d => d.IsApplicable()))
                break;
            
            // Cases where the paragraph is ended
            if (PreviousLineWithTwoWhitespaces() || PreviousLineIsNewLine())
            {
                break;
            }
            
            if (!IsParseCompleted)
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

    private bool PreviousLineWithTwoWhitespaces()
    {
        return Check(-1, MarkdownTokenType.NewLine)
           && Check(-2, MarkdownTokenType.Whitespace)
           && Check(-3, MarkdownTokenType.Whitespace);
    }
    
    private bool PreviousLineIsNewLine()
    {
        return Check(-1, MarkdownTokenType.NewLine)
            && Check(-2, MarkdownTokenType.NewLine);
    }

    private ListBlock ReadOrderedList()
    {
        var rows = ReadListRows([
            MarkdownTokenType.Number,
            MarkdownTokenType.Dot,
            MarkdownTokenType.Whitespace]);

        return new ListBlock
        {
            Rows = rows,
            IsOrdered = true,
        };
    }

    private ListBlock ReadUnorderedList()
    {
        var rows = ReadListRows([
            MarkdownTokenType.MinusSign,
            MarkdownTokenType.Whitespace]);

        return new ListBlock
        {
            Rows = rows,
            IsOrdered = false,
        };
    }
    
    private ListRow[] ReadListRows(MarkdownTokenType[] startTokens)
    {
        var listNode = new ListNode();
        
        var previousElementSpacesCount = 0;
        while (!IsParseCompleted && MatchSequential(startTokens))
        {
            var elements = new List<MarkdownContentBlockElement>();
            
            // New line should continue list item, so that's code is here
            while (!IsParseCompleted)
            {
                var next = ReadPlain();
                var elementsToWrite = next.Elements;
                elements.AddRange(elementsToWrite);
                
                if (PreviousLineWithTwoWhitespaces())
                    elements.Add(new NewLineElement());
                else
                    break;
            }
                    
            listNode.Write(previousElementSpacesCount, elements.ToArray());
            
            previousElementSpacesCount = 0;
            while (Check(-previousElementSpacesCount - 1, MarkdownTokenType.Whitespace))
                previousElementSpacesCount++;
        }

        return listNode.GetListRows();
    }

    private class ListNode
    {
        private int? _initialIdent;
        private readonly List<ListRow> _elements = new();
        
        public void Write(int spacesCount, MarkdownContentBlockElement[] elements)
        {
            var ident = spacesCount / 3;
            _initialIdent ??= ident;
            
            var realIdent = Math.Abs(ident - _initialIdent.Value);
            var currentNode = _elements;
            for (var i = 0; i < realIdent; i++)
            {
                if (currentNode.Count == 0)
                    currentNode.Add(new ListRow { Elements = [] });
                currentNode = currentNode.Last().Children;
            }
            
            currentNode.Add(new ListRow { Elements = elements });
        }

        public ListRow[] GetListRows()
        {
            return _elements.ToArray();
        }
    }

    private CodeMarkdownContentBlock ReadCode()
    {
        Advance(3);
        
        var result = new List<MarkdownContentBlockElement>();
        
        string? language = null;
        if (Match(MarkdownTokenType.Word))
            language = Previous().Literal?.ToString();

        var started = false;
        
        while (
            !IsParseCompleted
            && !MatchSequential(MarkdownTokenType.Backtick, 3))
        {
            if (Check(MarkdownTokenType.NewLine) && !started)
            {
                Advance();
                continue;
            }
            
            started = true;
            var element = ReadPlainElement();
            result.Add(element);
        }

        return new CodeMarkdownContentBlock
        {
            Elements = result.Trim(Environment.NewLine).ToArray(),
            Language = language
        };
    }

    private MarkdownContentBlockElement[] ReadRowElements()
    {
        var result = new List<MarkdownContentBlockElement>();
        
        while (!IsParseCompleted && !Match(MarkdownTokenType.NewLine))
        {
            var element = ReadElement();
            result.Add(element);
        }
        
        return result.Trim(" ").ToArray();
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
        
        if (element.TokenType == MarkdownTokenType.NewLine)
            return new PlainMarkdownContentBlockElement
            {
                Content = Environment.NewLine,
            };
        
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
        var linkContent = new List<MarkdownContentBlockElement>();
        while (!IsRowEndReached() && !Match(MarkdownTokenType.RightSquareBracket))
            linkContent.Add(ReadElement());

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
            Link = linkContent.ToArray(),
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