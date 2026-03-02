using System.Text;

namespace Laraue.Interpreter.Markdown.Body;

public static class HeadingUtility
{
    public static StringBuilder GenerateHeadingId(StringBuilder text)
    {
        var sb = new StringBuilder();

        for (var index = 0; index < text.Length; index++)
        {
            var nextChar = text[index];
            if (nextChar == ' ')
                sb.Append('-');
            else if (char.IsUpper(nextChar))
                sb.Append(char.ToLower(nextChar));
            else
                sb.Append(nextChar);
        }

        return sb;
    }
}