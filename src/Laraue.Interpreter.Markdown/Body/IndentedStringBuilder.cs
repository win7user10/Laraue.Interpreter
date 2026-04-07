using System.Text;

namespace Laraue.Interpreter.Markdown.Body;

public class IndentedStringBuilder
{
    private const string NewLine = "\r\n";

    /// <summary>
    /// String used for ident in the builder.
    /// </summary>
    private const string Ident = "  ";

    /// <summary>
    /// Current ident of SQL builder. All new rows will inherit this ident.
    /// </summary>
    private int CurrentIdent { get; set; }

    /// <summary>
    /// All rows in <see cref="IndentedStringBuilder"/>.
    /// </summary>
    private List<IndentedStringBuilderRow> Rows { get; } = new ();

    /// <summary>
    /// Latest row of <see cref="IndentedStringBuilder"/>.
    /// </summary>
    private IndentedStringBuilderRow CurrentSqlBuilderRow => Rows.Last();

    private IndentedStringBuilder(IndentedStringBuilderRow sqlBuilderRow)
    {
        Rows.Add(sqlBuilderRow);
    }

    /// <summary>
    /// Initialize new <see cref="IndentedStringBuilder"/> with empty content.
    /// </summary>
    public IndentedStringBuilder()
    {
        StartNewRow();
    }

    private IndentedStringBuilder StartNewRow(string? value = null)
    {
        return StartNewRow(new IndentedStringBuilderRow(CurrentIdent, value));
    }
    
    private IndentedStringBuilder StartNewRow(IndentedStringBuilderRow sqlBuilderRow)
    {
        Rows.Add(sqlBuilderRow);

        return this;
    }

    private static string GetIdent(int ident)
    {
        return string.Concat(Enumerable.Range(0, ident).Select(_ => Ident));
    }
    
    /// <summary>
    /// Returns to the delegate instance of
    /// <see cref="IndentedStringBuilder"/> with increased ident.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public IndentedStringBuilder WithIdent(Action<IndentedStringBuilder> action)
    {
        if (Rows.Count == 1 && CurrentSqlBuilderRow.StringBuilder.Length == 0)
        {
            action(this);
            return this;
        }
        
        CurrentIdent++;

        StartNewRow();
            
        action(this);

        CurrentIdent--;

        return this;
    }
    /// <summary>
    /// Append string value to current <see cref="IndentedStringBuilder"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public IndentedStringBuilder Append(string value)
    {
        CurrentSqlBuilderRow.StringBuilder.Append(value);
        return this;
    }
    
    public IndentedStringBuilder Append(char value)
    {
        CurrentSqlBuilderRow.StringBuilder.Append(value);
        return this;
    }
    
    public IndentedStringBuilder Append(StringBuilder stringBuilder)
    {
        CurrentSqlBuilderRow.StringBuilder.Append(stringBuilder);
        return this;
    }
    
    /// <summary>
    /// Starts new row in <see cref="IndentedStringBuilder"/> with passed string value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public IndentedStringBuilder AppendNewLine(string? value = null)
    {
        return StartNewRow(value);
    }

    public void ExecuteForAllBesidesLast<T>(
        IEnumerable<T> values, 
        Action<T, int> actionForAll,
        Action<T, int> actionForFirst)
    {
        var valuesArray = values.ToArray();
            
        for (var i = 0; i < valuesArray.Length; i++)
        {
            actionForAll(valuesArray[i], i);

            if (i != valuesArray.Length - 1)
            {
                actionForFirst(valuesArray[i], i);
            }
        }
    }
    
    /// <summary>
    /// Get the final SQL code.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var fullSql = new StringBuilder();

        ExecuteForAllBesidesLast(Rows, (row, _) =>
        {
            var ident = GetIdent(row.Ident);
                
            fullSql.Append(ident)
                .Append(row.StringBuilder);
        }, (_, _) => fullSql.Append(NewLine));

        return fullSql.ToString();
    }
}