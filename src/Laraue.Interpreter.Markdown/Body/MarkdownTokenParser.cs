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
                IsApplicable = () => CheckSequential(MarkdownTokenType.MinusSign, 3),
                Read = ReadHr
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
                Read = ReadList
            },
            new ReadBlockDelegate
            {
                IsApplicable = () => CheckSequential(
                    MarkdownTokenType.MinusSign,
                    MarkdownTokenType.Whitespace),
                Read = ReadList
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

        var firstRowIsDivider = IsTableContentDivider(rows.First());
        var dividerRow = firstRowIsDivider
            ? rows.First()
            : rows.Count > 1 ? rows[1] : null;

        var alignments = dividerRow is null
            ? []
            : dividerRow.Cells.Select(GetColumnAlignment).ToArray();

        if (firstRowIsDivider)
        {
            return new TableContentBlock
            {
                Header = null,
                Rows = rows.Skip(1).ToArray(),
                ColumnAlignments = alignments,
            };
        }

        return new TableContentBlock
        {
            Header = rows.First(),
            Rows = rows.Skip(2).ToArray(),
            ColumnAlignments = alignments,
        };
    }

    private static TableColumnAlignment GetColumnAlignment(TableContentBlockCell cell)
    {
        var content = GetPlainContent(cell);
        var leftAlign = content.StartsWith(':');
        var rightAlign = content.EndsWith(':');

        return (leftAlign, rightAlign) switch
        {
            (true, true) => TableColumnAlignment.Center,
            (true, false) => TableColumnAlignment.Left,
            (false, true) => TableColumnAlignment.Right,
            _ => TableColumnAlignment.None,
        };
    }

    private static string GetPlainContent(TableContentBlockCell cell)
    {
        return string.Concat(cell.Elements
            .OfType<PlainMarkdownContentBlockElement>()
            .Select(e => e.Content));
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
    
    private MarkdownContentBlock ReadHr()
    {
        var elements = ReadRowElements();
        if (elements.Skip(3).All(e => e is PlainMarkdownContentBlockElement { Content: " "}))
            return new HrContentBlock();

        return new PlainMarkdownContentBlock { Elements = elements };
    }

    private bool IsTableContentDivider(TableContentBlockRow row)
    {
        return row.Cells.Length > 0 && row.Cells.All(IsDividerCell);
    }

    private static bool IsDividerCell(TableContentBlockCell cell)
    {
        if (cell.Elements.Length == 0 || cell.Elements.Any(e => e is not PlainMarkdownContentBlockElement))
            return false;

        var dashes = GetPlainContent(cell).Trim(':');
        return dashes.Length > 0 && dashes.All(c => c == '-');
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

    private static readonly MarkdownTokenType[] OrderedListMarker =
    [
        MarkdownTokenType.Number,
        MarkdownTokenType.Dot,
        MarkdownTokenType.Whitespace
    ];

    private static readonly MarkdownTokenType[] UnorderedListMarker =
    [
        MarkdownTokenType.MinusSign,
        MarkdownTokenType.Whitespace
    ];

    private ListBlock ReadList()
    {
        var rows = ReadListRows();

        return new ListBlock
        {
            Rows = rows,
            IsOrdered = rows.Length == 0 || rows[0].IsOrdered,
        };
    }

    private bool TryMatchListMarker(out bool isOrdered)
    {
        if (MatchSequential(OrderedListMarker))
        {
            isOrdered = true;
            return true;
        }

        if (MatchSequential(UnorderedListMarker))
        {
            isOrdered = false;
            return true;
        }

        isOrdered = false;
        return false;
    }

    private ListRow[] ReadListRows()
    {
        var listNode = new ListNode();

        var previousElementSpacesCount = 0;
        while (!IsParseCompleted && TryMatchListMarker(out var isOrdered))
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

            listNode.Write(previousElementSpacesCount, isOrdered, elements.ToArray());

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

        public void Write(int spacesCount, bool isOrdered, MarkdownContentBlockElement[] elements)
        {
            var ident = spacesCount / 3;
            _initialIdent ??= ident;

            var realIdent = Math.Abs(ident - _initialIdent.Value);
            var currentNode = _elements;
            for (var i = 0; i < realIdent; i++)
            {
                if (currentNode.Count == 0)
                    currentNode.Add(new ListRow { Elements = [], IsOrdered = isOrdered });
                currentNode = currentNode.Last().Children;
            }

            currentNode.Add(new ListRow { Elements = elements, IsOrdered = isOrdered });
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
            language = Previous().Literal?.ToString() ?? Previous().Lexeme;

        while (
            !IsParseCompleted
            && !MatchSequential(MarkdownTokenType.Backtick, 3))
        {
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
        if (Check(MarkdownTokenType.Asterisk))
            return ReadItalicOrBoldElement(MarkdownTokenType.Asterisk);

        if (Check(MarkdownTokenType.Underscore))
            return ReadItalicOrBoldElement(MarkdownTokenType.Underscore);

        if (Check(MarkdownTokenType.Tilde))
            return ReadStrikethroughElement();

        if (Match(MarkdownTokenType.Backtick))
            return ReadBacktickElement();

        if (Check(MarkdownTokenType.LeftSquareBracket))
        {
            if (!HasClosingDelimiterOnLine(MarkdownTokenType.RightSquareBracket, 1))
                return ReadPlainElement();

            Advance();
            return ReadLink();
        }

        if (Check(MarkdownTokenType.Not) && Check(1, MarkdownTokenType.LeftSquareBracket))
        {
            Advance();
            return ReadImage();
        }

        return ReadPlainElement();
    }

    /// <summary>
    /// Returns true if the token type repeated <paramref name="width"/> times can be found
    /// somewhere later on the current row. Used to avoid opening an emphasis/link span
    /// that will never be closed, in which case the opening delimiter should be read as plain text.
    /// </summary>
    private bool HasClosingDelimiterOnLine(MarkdownTokenType tokenType, int width)
    {
        var offset = width;
        while (!Check(offset, MarkdownTokenType.NewLine) && !Check(offset, (MarkdownTokenType?)null))
        {
            if (CheckSequential(tokenType, width, offset))
                return true;

            offset++;
        }

        return false;
    }

    private bool CheckSequential(MarkdownTokenType tokenType, int count, int startOffset)
    {
        for (var i = 0; i < count; i++)
        {
            if (!Check(startOffset + i, tokenType))
                return false;
        }

        return true;
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
    
    /// <summary>
    /// Tracks currently open emphasis/strikethrough spans (token type + delimiter width) so that
    /// the same exact delimiter can't be re-opened while already open (which would be ambiguous),
    /// while still allowing different kinds of spans to nest, e.g. <c>**bold *and italic* text**</c>.
    /// </summary>
    private readonly Stack<(MarkdownTokenType TokenType, int Width)> _openEmphasis = new();

    private MarkdownContentBlockElement ReadItalicOrBoldElement(MarkdownTokenType tokenType)
    {
        if (_linkReadStarted)
            return ReadPlainElement();

        var isBold = CheckSequential(tokenType, 2);
        var width = isBold ? 2 : 1;

        if (tokenType == MarkdownTokenType.Underscore && IsIntrawordUnderscore(width))
            return ReadPlainElement();

        var marker = (tokenType, width);
        if (_openEmphasis.Contains(marker) || !HasClosingDelimiterOnLine(tokenType, width))
            return ReadPlainElement();

        Advance(width);
        _openEmphasis.Push(marker);
        var element = isBold
            ? (MarkdownContentBlockElement)ReadBoldElement(tokenType)
            : ReadItalicElement(tokenType);
        _openEmphasis.Pop();
        return element;
    }

    /// <summary>
    /// CommonMark's intraword underscore rule: an underscore surrounded by word characters on both
    /// sides (e.g. <c>bot_name_bot</c>) is not treated as an emphasis delimiter.
    /// </summary>
    private bool IsIntrawordUnderscore(int width)
    {
        return HasPrevious()
            && (Check(-1, MarkdownTokenType.Word) || Check(-1, MarkdownTokenType.Number))
            && (Check(width, MarkdownTokenType.Word) || Check(width, MarkdownTokenType.Number));
    }

    private MarkdownContentBlockElement ReadStrikethroughElement()
    {
        if (_linkReadStarted || !CheckSequential(MarkdownTokenType.Tilde, 2))
            return ReadPlainElement();

        var marker = (MarkdownTokenType.Tilde, 2);
        if (_openEmphasis.Contains(marker) || !HasClosingDelimiterOnLine(MarkdownTokenType.Tilde, 2))
            return ReadPlainElement();

        Advance(2);
        _openEmphasis.Push(marker);

        var elements = new List<MarkdownContentBlockElement>();
        while (!IsRowEndReached())
        {
            if (CheckSequential(MarkdownTokenType.Tilde, MarkdownTokenType.Tilde))
            {
                Advance(2);
                break;
            }

            elements.Add(ReadElement());
        }

        _openEmphasis.Pop();

        return new StrikethroughMarkdownContentBlockElement
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

    private ItalicMarkdownContentBlockElement ReadItalicElement(
        MarkdownTokenType tokenType)
    {
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
    
    private bool _linkReadStarted;
    private LinkCodeMarkdownContentBlockElement ReadLink()
    {
        _linkReadStarted = true;
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

        _linkReadStarted = false;
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