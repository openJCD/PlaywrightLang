#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;

namespace PlaywrightLang.LanguageServices;

public abstract class Node
{
    public abstract object Evaluate();
    public abstract override string ToString();
}

public class Add : Node
{
    // replace this with expression node
    public readonly Node Left;
    
    public readonly Node Right;

    public Add(Node left, Node right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override object Evaluate()
    {
        try
        {
            int l_val = int.Parse(Left.Evaluate().ToString());
            int r_val = int.Parse(Right.Evaluate().ToString());
            Parser.Log($"Addition: {ToString()}");
            return (l_val + r_val);
        }
        catch (Exception exception)
        {
            throw new Exception($"Unable to parse expression: {Left.Evaluate().ToString()} + {Right.Evaluate().ToString()}: {exception.Message}.");
        }
    }

    public override string ToString()
    {
        return $"({Left} + {Right})";
    }
}

public class Integer : Node {
    
    public int Value { get; private set; }

    public Integer(int value)
    {
        Value = value;
    }
    public override object Evaluate()
    {
        return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

public class Subtract : Node
{
    public readonly Node Left;
    public readonly Node Right;
    public Subtract(Node left, Node right)
    {
        this.Left = left;
        this.Right = right;
    }
    
    public override object Evaluate()
    {
        try
        {
            int l_val = int.Parse(Left.Evaluate().ToString());
            int r_val = int.Parse(Right.Evaluate().ToString());
            Parser.Log($"Subtraction: {ToString()}");
            return (l_val - r_val);
        }
        catch
        {
            throw new Exception($"Unable to parse expression: {Left.Evaluate().ToString()} - {Right.Evaluate().ToString()}. Expected integer values.");
        }
    }
    public override string ToString()
    {
        return "(" + Left.ToString() + " - " + Right.ToString() + ")";
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
    public Node ParseExpression()
    {
        
        // Node left = ParseTerm() - traverse the tree recursively!
        // temporary code to test addition capabilities of language
        while (Peek().Type != TokenType.Null)
        {
            Token l_val = Consume();
            if (l_val.Type == TokenType.IntLiteral)
            {
                Token opr = Consume();
                if (IsBinop(opr.Type))
                {
                    
                    switch (opr.Type)
                    {
                        // each statement here technically has the wrong order - first should be a ParseExpression, then a ParseTerm.
                        // this will hopefully be fixed later when ParseTerm and ParseFactor are implemented. 
                        case TokenType.Plus:
                            return new Add(new Integer(int.Parse(l_val.Value)), ParseExpression());
                        case TokenType.Minus:
                            return new Subtract(new Integer(int.Parse(l_val.Value)), ParseExpression());
                        case TokenType.IntLiteral:
                            return new Integer(int.Parse(l_val.Value));
                        default:
                            Log($"Problem: no operator @ {_tokenIndex}, instead found {opr}");
                            break;
                    }
                }
                else
                {
                    return new Integer(int.Parse(l_val.Value));
                }
                if (Peek().Type == TokenType.Newline)
                {
                    _currentLine++;
                    break;
                }
            }
        }
        Log("Reached EOF without finding an int literal.");
        return null;
    }

    public void ParseTerm()
    {
        
    }

    public void ParseFactor()
    {
        
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

    public static void Log(string message)
    {
        Console.WriteLine($"• Playwright Parser: {message}");
    }

    bool IsBinop(TokenType op)
    {
        return op == TokenType.Plus || op == TokenType.Minus || op == TokenType.Multiply || op == TokenType.Divide;
    }
}