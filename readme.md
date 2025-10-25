# Laraue.Interpreter  

The project is the just set of tools to implement a new Interpreter in CSharp.
Based on the examples from [Crafting interpreters book](https://craftinginterpreters.com)

## Laraue.Interpreter.Scanning

Contains the scanner base class. Create the token enum

```csharp
public enum MdTokenType
{
    /// <summary>
    /// '*'
    /// </summary>
    Asterisk,
    
    /// <summary>
    /// '**'
    /// </summary>
    DoubleAsterisk,
    
    /// <summary>
    /// '`'
    /// </summary>
    Backtick,
    
    ...
}
```

and implement the scanner

```csharp
public class MdTokenScanner(string input)
{
    protected override bool TryProcessNextChar(char nextChar)
    {
        switch (nextChar)
        {
            case '*':
                AddToken(PopNextCharIf(c => c == '*') ? MdTokenType.DoubleAsterisk : MdTokenType.Asterisk);
                return true;
            case '`':
                AddToken(MdTokenType.Backtick);
                return true;
            
            ...
        }
}
```

now you can get the token scan result for the string
```csharp
var scanner = new MdTokenScanner(markdownFile);
var result = scanner.ScanTokens();
```

## Laraue.Interpreter.Parsing

Contains the parser base class. Create the parsing result class
```csharp
public class MdTokenExpr
{
    public required MdHeader[] Headers { get; set; }
    public required ContentBlock[] Content { get; set; }
}
```

And implement it like this

```csharp
public class MdTokenParser : TokenParser<MdTokenType, MdTokenExpr>
{
    protected override MdTokenExpr ParseInternal()
    {
        MdHeader[] headers = [];
        var contentBlocks = new List<ContentBlock>();
        
        Skip(MdTokenType.NewLine);
        if (CheckSequential(MdTokenType.MinusSign, 3))
        {
            headers = ConsumeHeaders(); // Consume headers implementation
        }
        
        while (!IsParseCompleted)
        {
            contentBlocks.Add(ReadNewLineBlock()); // Implement md raw reading
        }

        return new MdTokenExpr
        {
            Headers = headers,
            Content = contentBlocks.ToArray(),
        };
    }
}
```

now the token sequence from the scanner can be parsed
```csharp
var mdTokenParser = new MdTokenParser(result.Tokens);
var parseResult = mdTokenParser.Parse();
```