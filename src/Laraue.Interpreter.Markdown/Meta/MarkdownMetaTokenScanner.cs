using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Markdown.Meta;

public class MarkdownMetaTokenScanner(string input)
    : TokenScanner<MarkdownMetaTokenType>(input)
{
    protected override MarkdownMetaTokenType NewLineTokenType => MarkdownMetaTokenType.NewLine;
    protected override bool AddNewLineTokens => true;

    private bool _metaStarted;
    private bool _metaFinished;

    protected override bool TryProcessNextCharInternal(char nextChar)
    {
        switch (nextChar)
        {
            case ' ':
                return true;
            default:
                HandleNonWhitespaceChar(nextChar);
                return true;
        }
    }

    private void HandleNonWhitespaceChar(char nextChar)
    {
        // Try read metadata
        if (nextChar == '-')
        {
            ReadMetaDelimiter();
            return;
        }
        
        // If any char consumed but the meta is not started, there is no metadata in file
        if (!_metaStarted)
        {
            ReadContent();
            return;
        }
        
        // If meta is finished just read the whole content remained
        if (_metaFinished)
        {
            ReadContent();
            return;
        }
        
        switch (nextChar)
        {
            case ':':
                AddToken(MarkdownMetaTokenType.Delimiter);
                return;
            case '[':
                AddToken(MarkdownMetaTokenType.ArrayStart);
                return;
            case ']':
                AddToken(MarkdownMetaTokenType.ArrayEnd);
                return;
            case ',':
                AddToken(MarkdownMetaTokenType.Comma);
                return;
            default:
                AddWordOrNumber();
                return;
        }
    }

    private void ReadMetaDelimiter()
    {
        if (Check(0, '-') && Check(1, '-'))
        {
            AddToken(MarkdownMetaTokenType.MetaDelimiter);
            Advance(2);
            
            if (!_metaStarted)
                _metaStarted = true;
            else
                _metaFinished = true;
            return;
        }
        
        AddWordOrNumber();
    }

    private void ReadContent()
    {
        while (PopNextCharIf(_ => true));
        var text = GetCurrentScanValue();
        AddToken(MarkdownMetaTokenType.Content, text.ToString());
    }
    
    private void AddWordOrNumber()
    {
        // Check if only whitespaces remained then finish
        while (PopNextCharIf(ch => IsDigit(ch) || IsAlpha(ch) || ch == ' ' || ch == '_'))
        {
        }
        
        var text = GetCurrentScanValue().Trim();
        AddToken(MarkdownMetaTokenType.Word, text.ToString());
    }
}