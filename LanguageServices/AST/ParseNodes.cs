using System;
using System.Collections.Generic;
using System.Text;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;


public class ExitNode : Node
{
    public override object Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        return AddSpaces(level, "simple expression: exit");
    }
    
}
public class VoidNode : Node
{
    public override object Evaluate(ScopedSymbolTable scope) { return null; }
    public override string ToPrettyString(int level)
    {
        return AddSpaces(level, "void");
    }
}

public class GlobalAssigmnent(Node variableName, Node assignedValue, PwState state)
    : Node
{
    private Node VariableName { get; set; } = variableName;
    private Node AssignedValue { get; set; } = assignedValue;

    private PwState _state = state;

    public override object Evaluate(ScopedSymbolTable scope)
    {
        object right = AssignedValue.Evaluate(scope);
        string left = VariableName.Evaluate(scope).ToString();
        
        var variable = new PwPrimitive(left, right, scope);
        return variable;
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "assignment: (\r\n");
        s += AddSpaces(level + 1, $"left: \r\n" +
                                  $"{VariableName.ToPrettyString(level + 1) },\r\n");
        s += AddSpaces(level + 1, $"right: \r\n" +
                                  $"{AssignedValue.ToPrettyString(level + 1)},\r\n");
        s += AddSpaces(level, ")");
        return s;
    }
}

public class ActorAssignment(string name, string type) : Node
{
    public string ActorName = name;
    public string ActorType = type;
    public override object Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "actor casting: (\r\n");
        s += AddSpaces(level + 1, $"name: {ActorName},\r\n");
        s += AddSpaces(level + 1, $"type: {ActorType},\r\n");
        s += AddSpaces(level, ")");
        return s;
    }
}


public class PostfixChain(Node[] operators) : Node
{
    public override object Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "postfix: (\r\n");
        foreach (Node op in operators)
        {
            s += $"{op.ToPrettyString(level+1)}, \r\n";
        }

        s += AddSpaces(level, ")");
        return  s;
    }
}

