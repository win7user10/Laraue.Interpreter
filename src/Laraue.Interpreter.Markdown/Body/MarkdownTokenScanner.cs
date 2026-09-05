using Laraue.Interpreter.Scanning;

namespace Laraue.Interpreter.Markdown.Body;

public class MarkdownTokenScanner(string input)
    : TokenScanner<MarkdownTokenType>(input.AsMemory())
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
            case '>':
                AddToken(MarkdownTokenType.GreaterThan);
                return true;
            case '~':
                AddToken(MarkdownTokenType.Tilde);
                return true;
            case ':':
                AddToken(MarkdownTokenType.Colon);
                return true;
            case '\\':
                AddEscapedCharOrBackslash();
                return true;
            default:
                AddWordOrNumber();
                return true;
        }
    }

    private static bool IsEscapableChar(char c) =>
        c is '*' or '_' or '`' or '#' or '[' or ']' or '(' or ')'
            or '-' or '!' or '"' or '.' or '>' or '|' or '~' or ':' or '\\';

    private void AddEscapedCharOrBackslash()
    {
        if (Check(0, IsEscapableChar))
        {
            Advance();
            var scanned = GetCurrentScanValue();
            AddToken(MarkdownTokenType.EscapedChar, scanned[1].ToString());
            return;
        }

        AddToken(MarkdownTokenType.Word);
    }

    private void AddWordOrNumber()
    {
        var startsWithDigit = Check(-1, IsDigit);
        while (PopNextCharIf(IsDigit));

        // Digit string found. No need to pass an explicit Literal here: the scanned span only
        // ever contains digits, so it's identical to the Lexeme that AddToken already captures.
        if (startsWithDigit && !Check(0, IsAlpha))
        {
            AddToken(MarkdownTokenType.Number);
            return;
        }

        // Usual string. Same reasoning as above: word chars are alnum-only, so Trim() here
        // never actually removes anything and the Lexeme already equals this value.
        while (PopNextCharIf(IsWordChar));
        AddToken(MarkdownTokenType.Word);
    }

    private bool IsWordChar(char ch)
    {
        return IsDigit(ch) || IsAlpha(ch);
    }
}