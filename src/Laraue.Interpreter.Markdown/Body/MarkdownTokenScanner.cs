using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Markdown.Body;

public class MarkdownTokenScanner(string input)
    : TokenScanner<MarkdownTokenType>(input)
{
    protected override MarkdownTokenType NewLineTokenType => MarkdownTokenType.NewLine;
    protected override bool AddNewLineTokens => false;
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
            default:
                AddWordOrNumber();
                return true;
        }
    }
    
    private void AddWordOrNumber()
    {
        while (PopNextCharIf(IsWordChar));
        
        var text = GetCurrentScanValue().Trim();
        
        AddToken(MarkdownTokenType.Word, text.ToString());
    }

    private bool IsWordChar(char ch)
    {
        var otherChars = new[] { 'a', 'b' };
        return IsDigit(ch) || IsAlpha(ch) || otherChars.Contains(ch);
    }
}