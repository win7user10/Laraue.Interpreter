using Laraue.Interpreter.Markdown.Meta;

namespace Laraue.Interpreter.Markdown.UnitTests.Infrastructure;

public static class CheckHeaderUtility
{
    public static void Check(
        MarkdownHeader header,
        string propertyName,
        int headerLineNumber,
        object value)
    {
        Assert.Equal(propertyName, header.PropertyName);
        Assert.Equal(headerLineNumber, header.LineNumber);
        Assert.Equal(value, header.Value);
    }
}