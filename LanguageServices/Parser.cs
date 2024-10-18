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
            Parser.Log($"Unable to parse expression: {Left} + {Right}: {exception.Message}.");
            return null;
        }
    }

    public override string ToString()
    {
        return $"({Left} + {Right})";
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
public class Negative : Node
{
    public readonly Node Term;

    public Negative(Node term)
    {
        this.Term = term;
    }

    public override object Evaluate()
    {
        return -(int)Term.Evaluate();
    }

    public override string ToString()
    {
        return $"-({Term})";
    }
}
public class Multply : Node
{
    public readonly Node Left;
    public readonly Node Right;

    public Multply(Node left, Node right)
    {
        this.Left = left;
        this.Right = right;
    }

    public override object Evaluate()
    {
        Parser.Log($"Multiply: {this}");
        return (int)Left.Evaluate() * (int)Right.Evaluate();
    }

    public override string ToString()
    {
        return $"({Left} * {Right})";
    }
}
public class Divide : Node
{
    public readonly Node Left;
    public readonly Node Right;

    public Divide(Node left, Node right)
    {
        this.Left = left;
        this.Right = right;
    }
    
    public override object Evaluate()
    {
        Parser.Log($"Division: {this}");
        return (int)Left.Evaluate() / (int)Right.Evaluate();
    }

    public override string ToString()
    {
        return $"({Left} / {Right})";
    }
}
public class Name : Node
{
    public readonly string Identifier;

    public Name(string identifier)
    {
        this.Identifier = identifier;
    }
    // change this to access some kind of stack!
    public override object Evaluate()
    {
        Parser.Log($"Found Name: {Identifier}");
        try
        {
            PwObject obj = Parser.Objects[Identifier];
            if (obj.Type == PwObjectType.StringVariable)
                return obj.Data.ToString();
            if (obj.Type == PwObjectType.IntVariable)
                return (int)obj.Data;
        }
        catch
        {
            Parser.Log($"Not implemented: tried to evaluate name.");
        }
        return null;
    }
    public override string ToString() => Identifier;
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

public class Parser
{
    private List<Token> _tokens;
    private int _tokenIndex;
    private int _currentLine=1;
    
    internal static Dictionary<string, PwObject> Objects = new Dictionary<string, PwObject>();
    
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }
    public Node ParseExpression()
    {
        while (Peek().Type != TokenType.Null)
        { 
            Node l_val = ParseTerm();
            while (Peek().Type != TokenType.Newline)
            {
                Token t_op = Consume();
                switch (t_op.Type)
                {
                    // each statement here technically has the wrong order - first should be a ParseExpression, then a ParseTerm.
                    // this will hopefully be fixed later when ParseTerm and ParseFactor are implemented. 
                    case TokenType.Plus:
                        return new Add(l_val, ParseTerm());
                    case TokenType.Minus:
                        return new Subtract(l_val, ParseTerm());
                }

                if (Peek().Type == TokenType.Newline)
                {
                    _currentLine++;
                    break;
                }
            }
            return l_val;

        }
        return null;
    }

    public Node ParseTerm()
    {
        while (Peek().Type != TokenType.Null)
        {
            Node l_val = ParseFactor();
            while (Peek().Type != TokenType.Newline && Peek().Type != TokenType.Null)
            {
                Token next = Peek(0);
                if (next.Type == TokenType.Multiply || next.Type == TokenType.Divide)
                {
                    next = Consume();
                    switch (next.Type)
                    {
                        case TokenType.Multiply:
                            return new Multply(l_val, ParseFactor());
                        case TokenType.Divide:
                            return new Divide(l_val, ParseFactor());
                        default: break;
                    }
                }
            }
            return l_val;
        } 
        return null;
    }

    public Node ParseFactor()
    {
        while (Peek().Type != TokenType.Newline && Peek().Type != TokenType.Null)
        {
            Token t_current = Consume();
            switch (t_current.Type)
            {
                case TokenType.Minus:
                    return new Negative(ParseFactor());
                case TokenType.Name:
                    return new Name(t_current.Value);
                case TokenType.IntLiteral: 
                    return new Integer(int.Parse(t_current.Value));
                case TokenType.LParen:
                    Node inner = ParseExpression();
                    Token t_next = Consume();
                    if (t_next.Type != TokenType.RParen)
                    {
                        ThrowError($"Expected ')', got {t_next.Type}");
                        return null;
                    }
                    return inner;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Increment the current token index and get token
    /// </summary>
    /// <returns>Current token before index is incremented</returns>
    Token Consume()
    {
        if (Peek(0).Type != TokenType.Null)
            return _tokens.ElementAt(_tokenIndex++);
        return Token.None;
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
    public static T GetVariable<T>(string name)
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