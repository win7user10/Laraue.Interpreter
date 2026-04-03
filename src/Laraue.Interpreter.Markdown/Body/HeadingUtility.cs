using System.Text;
using Laraue.Interpreter.Markdown.Body.BlockElements;

namespace Laraue.Interpreter.Markdown.Body;

public static class HeadingUtility
{
    public static Heading GenerateHeading(MarkdownContentBlockElement[] elements)
    {
        var source = new Source();

        foreach (var element in elements)
            AddElementContent(source, element);

        return new Heading
        {
            Title = source.TitleBuilder.ToString(),
            Id = source.IdBuilder
        };
    }

    private static void AddElementContent(Source source, MarkdownContentBlockElement element)
    {
        if (element is PlainMarkdownContentBlockElement plain)
        {
            source.TitleBuilder.Append(plain.Content);
            AppendToId(source.IdBuilder, plain.Content);
            return;
        }

        if (element is LinkCodeMarkdownContentBlockElement link)
        {
            foreach (var child in link.Link)
                AddElementContent(source, child);
            
            return;
        }
    }

    private static void AppendToId(StringBuilder sb, string content)
    {
        foreach (var nextChar in content)
        {
            if (nextChar == ' ')
                sb.Append('-');
            else if (char.IsUpper(nextChar))
                sb.Append(char.ToLower(nextChar));
            else
                sb.Append(nextChar);
        }
    }

    private class Source
    {
        public StringBuilder TitleBuilder = new();
        public StringBuilder IdBuilder = new();
    }

    public class Heading
    {
        public required string Title { get; set; }
        public required StringBuilder Id { get; set;  }
    }
}