using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace PlaywrightLang.LanguageServices;

public abstract class Node
{
    public abstract PwObject Evaluate(ScopedSymbolTable scope);
    
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
    public override PwObject Evaluate(ScopedSymbolTable scope) { return null; }
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

    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        foreach (Node _nd in _nodes)
        {
            _nd.Layer = Layer + 1;
            _nd.Evaluate(scope); 
        }

        Parser.Log("parsed chunk successfully");
        return null;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("\nbegin chunk:");
        foreach (Node nd in _nodes)
        {
            sb.Append(nd.BuildIndentedString());
        }
        return BuildIndentedString(sb.AppendLine("end chunk").ToString()); 
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

    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        foreach (Node _nd in _nodes)
        {
            _nd.Evaluate(scope); 
        }

        return null;
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

    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        object right = AssignedValue.Evaluate(scope);
        string left = VariableName.Evaluate(scope).ToString();
        
        var variable = new PwPrimitive(left, right, scope);
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
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        _state.SetActor(ActorName, ActorType);
        return null;
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
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        foreach (Node nd in FunctionCalls)
        {
            nd.Evaluate(scope);
            nd.Layer = Layer + 1;
        }

        return null;
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

public class FunctionCall(string id, ParamExpressions args, PwState state) : Node
{
    private string name = id;
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }
    public override string ToString()
    {
        return $"function call {id}: \n{args}";
    }
}

public class FunctionBlock(string id, string for_actor, Name[] args, Node[] instructions, PwState state) : Node
{
    Node[] _instructions = instructions;
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        List<string> argumentNames = new();
        foreach (var arg in args)
        {
            argumentNames.Add(arg.ToString());
        }

        var myself = new PwFunction(id, argumentNames.ToArray(), instructions, for_actor);
        state.RegisterPwFunction(myself, for_actor);
        return myself;
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
            sb.Append(nd);
        }
        return  BuildIndentedString(sb.AppendLine().ToString());
    }
}

public class ReturnStmt(string parentFunc, Node retValue) :  Node
{
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        return retValue.Evaluate(scope);
    }

    public override string ToString()
    {
        return BuildIndentedString($"\nreturn from {parentFunc} with {retValue}");
    }
}

public class DotOperator (string identifier) : Node
{
    private string Identifier = identifier;
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        string s = $"access: {identifier}";
        return s;
    }
}

public class Postfix(Node[] operators) : Node
{
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        string s = "postfix: (";
        foreach (Node op in operators)
        {
            s += $"{op},";
        }

        s += ")";
        return BuildIndentedString(s);
    }
}

public class ParamExpressions(Node[] parameters) : Node
{
    private Node[] Parameters = parameters;

    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        string s = $"params: (";
        foreach (Node param in Parameters)
        {
            s += $"{param}";
        }

        s += ')';
        return s;
    }
}

#region math
public class Add : Node
{
    public readonly Node Left;
    
    public readonly Node Right;

    public Add(Node left, Node right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        try
        {
            int l_val = (int)((Left.Evaluate(scope) as PwPrimitive).GetData());
            int r_val = (int)((Right.Evaluate(scope) as PwPrimitive).GetData());
            Parser.Log($"Addition: {ToString()}");
            return new PwPrimitive($"eval_{l_val}+{r_val}", l_val + r_val, scope);
        }
        catch (Exception exception)
        {
            Parser.Log($"Unable to parse expression: {Left} + {Right}: {exception.Message}. (Likely a type mismatch.)");
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
    
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        try
        {
            int l_val = (int)((Left.Evaluate(scope) as PwPrimitive).GetData());
            int r_val = (int)((Right.Evaluate(scope) as PwPrimitive).GetData());
            Parser.Log($"Subtraction: {ToString()}");
            return new PwPrimitive($"eval_{l_val}-{r_val}", l_val - r_val, scope);
        }
        catch
        {
            throw new Exception($"Unable to parse expression: {Left.Evaluate(scope).ToString()} - {Right.Evaluate(scope).ToString()}. Expected integer values.");
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

    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        return new PwPrimitive($"negate_{Term}", -(int)((Term.Evaluate(scope) as PwPrimitive).GetData()), scope);
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

    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        int l_val = (int)((Left.Evaluate(scope) as PwPrimitive).GetData());
        int r_val = (int)((Right.Evaluate(scope) as PwPrimitive).GetData());
        Parser.Log($"Multiply: {this}");
        return new PwPrimitive($"eval_{l_val} * {r_val}", l_val * r_val, scope); 
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

    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        int l_val = (int)((Left.Evaluate(scope) as PwPrimitive).GetData());
        int r_val = (int)((Right.Evaluate(scope) as PwPrimitive).GetData());
        Parser.Log($"Divide: {this}");
        return new PwPrimitive($"eval_{l_val} / {r_val}", l_val / r_val, scope); 
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
    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        Parser.Log($"Found Name: {Identifier}");

        return scope.Lookup(Identifier);
    }
    public override string ToString() => Identifier;
}
public class Integer(int value) : Node
{
    public int Value { get; private set; } = value;

    public override PwObject Evaluate(ScopedSymbolTable scope)
    {
        return new PwPrimitive("new_int", Value, scope);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
public class StringLit(string value) : Node
{
    public readonly string Value = value;

    public override PwObject Evaluate(ScopedSymbolTable scope) => new PwPrimitive("new_string", Value, scope);

    public override string ToString() => $"\"{Value}\"";
}
#endregion