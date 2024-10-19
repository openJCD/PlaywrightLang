using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
namespace PlaywrightLang.LanguageServices;

public abstract class Node
{
    public abstract object Evaluate();
    public abstract override string ToString();
}

public class Chunk : Node
{
    List<Node> _nodes = new List<Node>();
    public Chunk(params Node[] nodes)
    {
        foreach (Node nd in nodes)
        {
            _nodes.Add(nd);
        }
    }

    public override object Evaluate()
    {
        foreach (Node _nd in _nodes)
        {
            _nd.Evaluate(); 
        }
        return 1;
    }

    public override string ToString()
    {
        return Convert.ToString(_nodes);
    }
}
public class Block : Chunk {}
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
public class Multiply(Node left, Node right) : Node
{
    public readonly Node Left = left;
    public readonly Node Right = right;

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
public class Divide(Node left, Node right) : Node
{
    public readonly Node Left = left;
    public readonly Node Right = right;

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
public class Name(string identifier, Parser parseState) : Node
{
    public readonly string Identifier = identifier;
    private readonly Parser _parseState = parseState;

    // change this to access some kind of stack!
    public override object Evaluate()
    {
        Parser.Log($"Found Name: {Identifier}");
        
        if (!_parseState.VariableExists(Identifier))
        {
            _parseState.ThrowError($"Variable, function or sequence {identifier} not exist in current context.");
            return null;
        }
        return _parseState.Globals[Identifier].Data;
    }
    public override string ToString() => Identifier;
}
public class Integer(int value) : Node
{
    
    public int Value { get; private set; } = value;

    public override object Evaluate()
    {
        return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
public class StringLit(string value) : Node
{
    public readonly string Value = value;

    public override object Evaluate() => Value;

    public override string ToString() => $"\"{Value}\"";
}