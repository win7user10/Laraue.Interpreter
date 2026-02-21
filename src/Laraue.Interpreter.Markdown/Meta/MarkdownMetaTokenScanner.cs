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
        if (_metaFinished)
        {
            ReadContent();
            return true;
        }
        
        switch (nextChar)
        {
            case ' ':
                return true;
            case ':':
                AddToken(MarkdownMetaTokenType.Delimiter);
                return true;
            case '[':
                AddToken(MarkdownMetaTokenType.ArrayStart);
                return true;
            case ']':
                AddToken(MarkdownMetaTokenType.ArrayEnd);
                return true;
            case ',':
                AddToken(MarkdownMetaTokenType.Comma);
                return true;
            case '-':
                ReadMetaDelimiter();
                return true;
            default:
                AddWordOrNumber();
                return true;
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