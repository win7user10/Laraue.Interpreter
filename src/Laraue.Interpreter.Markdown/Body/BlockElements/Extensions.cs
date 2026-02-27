namespace Laraue.Interpreter.Markdown.Body.BlockElements;

public static class Extensions
{
    extension(IEnumerable<MarkdownContentBlockElement> items)
    {
        public IEnumerable<MarkdownContentBlockElement> Trim(string content)
        {
            var result = new List<MarkdownContentBlockElement>();

            var lastWhitespaces = new List<MarkdownContentBlockElement>();
            
            MarkdownContentBlockElement? previous = null;
            foreach (var item in items)
            {
                if (ShouldBeTrimmed(item, content))
                {
                    if (previous is null)
                        continue;
                    if (ShouldBeTrimmed(previous, content))
                        continue;
                    lastWhitespaces.Add(item);
                        continue;
                }

                if (lastWhitespaces.Count > 0)
                {
                    result.AddRange(lastWhitespaces);
                    lastWhitespaces.Clear();
                }
                
                previous = item;
                result.Add(item);
            }

            return result.ToArray();
        }
    }

    private static bool ShouldBeTrimmed(MarkdownContentBlockElement element, string content)
    {
        return element is PlainMarkdownContentBlockElement e && e.Content == content;
    }
}