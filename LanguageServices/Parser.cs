#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;

namespace PlaywrightLang.LanguageServices;

public abstract class NodeExpr
{
    public Token Token { get; protected set; }
    public Dictionary<string, PwObject> Objects { get; private set; } = new Dictionary<string, PwObject>();
    
    public abstract Token Evaluate();
}

public class Add : NodeExpr
{
    public int Value => int.Parse(Token.Value);
    public override Token Evaluate()
    {
        throw new NotImplementedException();
    }
}

public class Subtract : NodeExpr
{
    public override Token Evaluate()
    {
        throw new NotImplementedException();
    }
}
public class Parser
{
    private List<Token> _tokens;
    private int _tokenIndex;
    private int _currentLine=1;
    
    public Dictionary<string, PwObject> Objects = new Dictionary<string, PwObject>();
    
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public void DoParse()
    {
    }
    Token ComputeExpression(int initialPrecedence = 1)
    {
        return ComputeNumber();
    } 
    Token ComputeNumber()
    {
        return Token.None;
    }
    /// <summary>
    /// Increment the current token index and get token
    /// </summary>
    /// <returns>Current token before index is incremented</returns>
    Token Consume()
    {
        return _tokens.ElementAt(_tokenIndex++);
    }
    /// <summary>
    /// Look at the token with the given index offset.
    /// </summary>
    /// <param name="ahead">Index offset to look at (can be negative)</param>
    /// <returns>Peeked token, or Token.None if EOF encountered</returns>
    Token Peek(int ahead = 1)
    {
        if (_tokenIndex + ahead >= _tokens.Count || _tokenIndex + ahead < 0)
            return Token.None;
        
        return _tokens.ElementAt(_tokenIndex + ahead);
    }
    public T GetVariable<T>(string name)
    {
        Objects.TryGetValue(name, out PwObject value);
        return value.Get<T>();
    }
    void ThrowError(string err)
    {
        StringBuilder message = new StringBuilder();
        message.Append($"Parse error on line '{_currentLine}': {err}").AppendLine();
        Console.Error.Write(message.ToString());
    }
}