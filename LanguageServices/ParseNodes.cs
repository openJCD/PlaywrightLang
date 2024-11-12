using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlaywrightLang.LanguageServices;

public abstract class Node
{
    // TODO: change signature so a scope can be passed to each node.
    public abstract object Evaluate();
    public abstract override string ToString();

    public int Layer { get; set; } = 0;

    public string BuildIndentedString()
    {
        return BuildIndentedString(ToString());
    }

    public string BuildIndentedString(string text)
    {
        var sb = new StringBuilder();
        
        for (int i = 1; i <= Layer; i++)
        {
            sb.Append("  ");
        }
        sb.AppendLine(text);
        return sb.ToString();
    }
}

public class VoidNode : Node
{
    public override object Evaluate() { return "void"; }
    public override string ToString()
    {
        return "void";
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
            _nd.Layer = Layer + 1;
            _nd.Evaluate(); 
        }

        Parser.Log("parsed chunk successfully");
        return ToString();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(BuildIndentedString("\nbegin chunk:"));
        foreach (Node nd in _nodes)
        {
            sb.Append(nd.BuildIndentedString());
        }
        return sb.Append(BuildIndentedString("end chunk")).ToString(); 
    }
}

public class Block : Node
{
    List<Node> _nodes = new();
    private string btype = "";
    public Block(string blockType, params Node[] nodes)
    {
        btype = blockType;
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

        return ToString();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"begin {btype} block:");
        foreach (Node nd in _nodes)
        {
            nd.Layer = Layer + 1;
            sb.Append(nd.BuildIndentedString());
        }
        return sb.Append(BuildIndentedString("end block")).ToString(); 
    }
}

public class GlobalAssigmnent(Node variableName, Node assignedValue, PwState state)
    : Node
{
    private Node VariableName { get; set; } = variableName;
    private Node AssignedValue { get; set; } = assignedValue;

    private PwState _state = state;

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

public class ActorAssignment(string name, string type, PwState state) : Node
{
    public string ActorName = name;
    public string ActorType = type;
    private PwState _state = state;
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

public class FunctionCall(string id, string caller, Name[] args, PwState state) : Node
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
        return callerActor.InvokeMethod(id, argsEvaluated);
    }

    public override string ToString()
    {
        return $"function call: {id}";
    }
}

public class FunctionBlock(string id, string for_actor, Name[] args, Node[] instructions, PwState state) : Node
{
    public override object Evaluate()
    {
        return new PwFunction(id, instructions, for_actor);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"begin function {id}").Append('(')
            .AppendJoin(',', args.ToList());

        sb.Append("):");
        foreach (Node nd in instructions)
        {
            nd.Layer = Layer + 1;
            sb.AppendLine(nd.BuildIndentedString());
        }
        return sb.AppendLine(BuildIndentedString("end function")).ToString();
    }
}

public class ReturnStmt(string parentFunc, Node retValue) : Node
{
    public override object Evaluate()
    {
        return retValue.Evaluate();
    }

    public override string ToString()
    {
        return BuildIndentedString($"return from {parentFunc} with {retValue}");
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

#region primitives

public class Name(string identifier, PwState state ) : Node
{
    public readonly string Identifier = identifier;

    // change this to access some kind of stack!
    public override object Evaluate()
    {
        Parser.Log($"Found Name: {Identifier}");

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