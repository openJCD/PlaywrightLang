using System;
using System.Collections.Generic;
using System.Text;

namespace PlaywrightLang.LanguageServices;

public abstract class Node
{
    public abstract object Evaluate();
    public abstract override string ToString();

    public int Layer { get; set; } = 0;

    public string BuildIndentedString()
    {
        var sb = new StringBuilder();
        sb.Append('|');
        for (int i = 0; i < Layer; i++)
        {
            sb.Append('-');
        }
        sb.Append('>').AppendLine(ToString());
        return sb.ToString();
    }
}

public class Chunk : Node
{
    List<Node> _nodes = new();
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
            _nd.Layer = Layer + 1;
        }

        Parser.Log(ToString());
        return ToString();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        foreach (Node nd in _nodes)
        {
            sb.AppendLine(nd.ToString());
        }
        return sb.Append("end").ToString(); 
    }
}

public class Block : Chunk
{
    public Block(params Node[] nodes) : base(nodes) { }
    public override string ToString() => $"block: \n{base.ToString()}";
}

public class GlobalAssigmnent(Node variableName, Node assignedValue, PlaywrightState state)
    : Node
{
    private Node VariableName { get; set; } = variableName;
    private Node AssignedValue { get; set; } = assignedValue;

    private PlaywrightState _state = state;

    public override object Evaluate()
    {
        object right = AssignedValue.Evaluate();
        string left = VariableName.Evaluate().ToString();
        PwObject variable = new PwObject(left, right);
        return variable;
    }

    public override string ToString()
    {
        return $"({VariableName} = {AssignedValue})";
    }
}

public class ActorAssignment(string name, string type, PlaywrightState state) : Node
{
    public string ActorName = name;
    public string ActorType = type;
    private PlaywrightState _state = state;
    public override object Evaluate()
    {
        _state.SetActor(ActorName, ActorType);
        return ActorName;
    }

    public override string ToString()
    {
        return $"Instantiated {ActorName} with type {ActorType}";
    }
}

public class Line(int num, string actorName, Node[] _functionCalls) : Node 
{
    public string ActorName = actorName;
    public Node[] FunctionCalls = _functionCalls;
    private int number = num;
    public override object Evaluate()
    {
        foreach (Node nd in FunctionCalls)
        {
            nd.Evaluate();
            nd.Layer = Layer + 1;
        }

        return ToString();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Line {number} {ActorName}: ");
        foreach (Node nd in FunctionCalls)
            sb.Append(nd.BuildIndentedString());
        return sb.ToString();
    }
}

public class FunctionCall(string id, string caller, Node[] args, PlaywrightState state) : Node
{
    Node[] Arguments = args;
    private string name = id;
    private string caller = caller;
    public override object Evaluate()
    {
        object[] argsEvaluated = new object[Arguments.Length];
        int i = 0;
        foreach (Node nd in Arguments)
        {
            argsEvaluated[i] = nd.Evaluate();
            i++;
            nd.Layer = Layer + 1;
        }
        PwActor callerActor = state.GetActor(caller);
        PwFunction fn = state.GetFunction(name);
        return state.RunMethod(callerActor, fn, argsEvaluated);
    }

    public override string ToString()
    {
        return "function call: " + id;
    }
}

#region math
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
#endregion

#region primmitives

public class Name(string identifier, PlaywrightState state ) : Node
{
    public readonly string Identifier = identifier;

    // change this to access some kind of stack!
    public override object Evaluate()
    {
        Parser.Log($"Found Name: {Identifier}");

        // change this later on: develop a scope system.
        return state.GetVariable(Identifier);
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
#endregion