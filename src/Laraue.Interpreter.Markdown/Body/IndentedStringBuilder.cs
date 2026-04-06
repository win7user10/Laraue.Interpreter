using System.Text;

namespace Laraue.Interpreter.Markdown.Body;

public class IndentedStringBuilder
{
    private readonly StringBuilder _stringBuilder = new ();
    private int _identation;

    public IndentedStringBuilder Append(string value)
    {
        for (var i = 0; i < _identation; i++)
        {
            _stringBuilder.Append(" ");
        }
        
        _stringBuilder.Append(value);
        return this;
    }
    
    public IndentedStringBuilder Append(char value)
    {
        return Append(value.ToString());
    }
    
    public IndentedStringBuilder Append(StringBuilder builder)
    {
        _stringBuilder.Append(builder);
        return this;
    }
    
    public IndentedStringBuilder AppendLine()
    {
        _stringBuilder.AppendLine();
            
        return this;
    }

    public IDisposable WithIdent()
    {
        _identation++;
        return new Identation(this);
    }

    private class Identation(IndentedStringBuilder builder) : IDisposable
    {
        public void Dispose()
        {
            builder._identation--;
        }
    }

    public override string ToString()
    {
        return _stringBuilder.ToString();
    }
}