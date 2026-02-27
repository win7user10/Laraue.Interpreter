using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Markdown.Body;

public class MarkdownTokenScanner(string input)
    : TokenScanner<MarkdownTokenType>(input)
{
    protected override MarkdownTokenType NewLineTokenType => MarkdownTokenType.NewLine;
    protected override bool AddNewLineTokens => true;
    protected override bool TryProcessNextCharInternal(char nextChar)
    {
        switch (nextChar)
        {
            case '#':
                AddToken(MarkdownTokenType.NumberSign);
                return true;
            case '*':
                AddToken(MarkdownTokenType.Asterisk);
                return true;
            case '`':
                AddToken(MarkdownTokenType.Backtick);
                return true;
            case '(':
                AddToken(MarkdownTokenType.LeftParenthesis);
                return true;
            case ')':
                AddToken(MarkdownTokenType.RightParenthesis);
                return true;
            case '-':
                AddToken(MarkdownTokenType.MinusSign);
                return true;
            case '_':
                AddToken(MarkdownTokenType.Underscore);
                return true;
            case '[':
                AddToken(MarkdownTokenType.LeftSquareBracket);
                return true;
            case ']':
                AddToken(MarkdownTokenType.RightSquareBracket);
                return true;
            case ' ':
                AddToken(MarkdownTokenType.Whitespace);
                return true;
            case '!':
                AddToken(MarkdownTokenType.Not);
                return true;
            case '|':
                AddToken(MarkdownTokenType.Pipe);
                return true;
            case '"':
                AddToken(MarkdownTokenType.Quote);
                return true;
            case '.':
                AddToken(MarkdownTokenType.Dot);
                return true;
            default:
                AddWordOrNumber();
                return true;
        }
    }
    
    private void AddWordOrNumber()
    {
        var startsWithDigit = Check(-1, IsDigit);
        while (PopNextCharIf(IsDigit));
        
        // Digit string found
        if (startsWithDigit && !Check(0, IsAlpha))
        {
            var stringValue = GetCurrentScanValue();
            AddToken(MarkdownTokenType.Number, stringValue.ToString());
            return;
        }
        
        // Usual string
        while (PopNextCharIf(IsWordChar));
        var text = GetCurrentScanValue().Trim();
        AddToken(MarkdownTokenType.Word, text.ToString());
    }

    private bool IsWordChar(char ch)
    {
        return IsDigit(ch) || IsAlpha(ch);
    }
}