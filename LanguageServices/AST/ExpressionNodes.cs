#nullable enable
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;

public class Expression(Node expr) : Node
{
    Node Inner = expr;
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        return expr.Evaluate(scope);
    }

    public bool IsTruthy(ScopedSymbolTable scope)
    {
        object result = expr.Evaluate(scope);
        return result != null || !result.Equals(false) || !result.Equals(0);
    }
    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "expression: (\r\n");
        s += $"{expr.ToPrettyString(level + 1)}\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

#region function

public class FunctionCall(Node path, ParamExpressions args) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }
    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, $"function call: (\r\n");
        s += $"{path.ToPrettyString(level + 1)},\r\n";
        s += $"{args.ToPrettyString(level + 1)}\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class ParamExpressions(ParamExpressions previous, Node current) : Node
{

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public PwInstance[] AsArray(ScopedSymbolTable scope)
    {
        List<PwInstance> result;
        if (previous == null)
        {
            result = new List<PwInstance>();
        }
        else
        {
            result = previous.AsArray(scope).ToList();
        }
        result.Add(current.Evaluate(scope));
        return result.ToArray();
    }
    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "param expr: (\r\n");
        if (previous != null && current != null)
        {
            s += $"{previous.ToPrettyString(level + 1)},\r\n";
            s += $"{current.ToPrettyString(level + 1)}\r\n";
            s += AddSpaces(level, ")");
        }
        else
        {
            s = AddSpaces(level, "");
        }

        return s;
    }
}

public class ParamNames(ParamNames previous, DeclarationParameter current) : Node
{
    private ParamNames Prev = previous;
    private DeclarationParameter Current = current;
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        return null;
    }
    // compile a dictionary of the prior ParamNames plus the current one.
    // the PwInstance? in each pair represents the default value 
    public Dictionary<string, PwInstance?> GetParameters(ScopedSymbolTable scope)
    {
        Dictionary<string, PwInstance?> parameters;
        if (Prev == null)
        {
             parameters = previous.GetParameters(scope);
        }
        else
        {
            parameters = new();
        }
        
        parameters[Current.Id] = Current.Evaluate(scope);
        return parameters;
    }
    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "named parameters: (\r\n");
        if (previous != null && current != null)
        {
            s += $"{previous.ToPrettyString(level + 1)},\r\n";
            s += $"{current.ToPrettyString(level + 1)}\r\n";
            s += AddSpaces(level, ")");
        }
        else
        {
            s = "none";
        }
        return s;    
    }
}

public class DeclarationParameter(Name id, Node? literal) : Node
{
    public string Id = id.Value;
    private Node? Literal = literal;
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        return Literal?.Evaluate(scope)!;
    }
    
    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "param: (\r\n");
        s += $"{id.ToPrettyString(level + 1) },\r\n";
        s += $"{literal?.ToPrettyString(level + 1)}\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

#endregion

#region assignment

public class AssignmentExpression(IQualifiedIdentifier lvalue, Node rvalue) : Node 
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "assignment: (\r\n");
        s += $"{lvalue.ToPrettyString(level + 1)}, \r\n";
        s += $"{rvalue.ToPrettyString(level + 1)} \r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class IncrementalAssignment(IQualifiedIdentifier lvalue, Node rvalue) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "incremental assignment: (\r\n");
        s += $"{lvalue.ToPrettyString(level + 1)}, \r\n";
        s += $"{rvalue.ToPrettyString(level + 1)} \r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
public class DecrementalAssignment(IQualifiedIdentifier lvalue, Node rvalue) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "decremental assignment: (\r\n");
        s += $"{lvalue.ToPrettyString(level + 1)}, \r\n";
        s += $"{rvalue.ToPrettyString(level + 1)} \r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class DivAssignment(IQualifiedIdentifier lvalue, Node rvalue) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "divisional assignment: (\r\n");
        s += $"{lvalue.ToPrettyString(level + 1)}, \r\n";
        s += $"{rvalue.ToPrettyString(level + 1)} \r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
public class MultAssignment(IQualifiedIdentifier lvalue, Node rvalue) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "multiplicative assignment: (\r\n");
        s += $"{lvalue.ToPrettyString(level + 1)}, \r\n";
        s += $"{rvalue.ToPrettyString(level + 1)} \r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

#endregion assignment

#region logic

public class LogicalOr(Node lvalue, Node rvalue) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "or expression: (\r\n");
        s += $"{lvalue.ToPrettyString(level + 1) },\r\n";
        s += $"{rvalue.ToPrettyString(level + 1)}\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class LogicalAnd(Node lvalue, Node rvalue) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "and expression: (\r\n");
        s += $"{lvalue.ToPrettyString(level + 1) },\r\n";
        s += $"{rvalue.ToPrettyString(level + 1)}\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class UnaryNot(Node value) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "not: (\r\n");
        s += $"{value.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

#endregion

#region operator

#region non-math
public class AccessOperator(Node left, Node right) : Node, IQualifiedIdentifier
{
    private Node Left = left;
    private Node Right = right;
    
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public void Set(object value)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "access member: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

#region equality
public class EqualOperator(Node left, Node right) : Node
{
    private Node Left = left;
    private Node Right = right;
    
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "equal: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class NotEqualOperator(Node left, Node right) : Node
{
    private Node Left = left;
    private Node Right = right;

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "not equal: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
#endregion equality

#region relational

public class GreaterThanOperator(Node left, Node right) : Node
{
    private Node Left = left;
    private Node Right = right;

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "greater than: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
public class LessThanOperator(Node left, Node right) : Node
{
    private Node Left = left;
    private Node Right = right;

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "less than: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
public class GreaterThanEqOperator(Node left, Node right) : Node
{
    private Node Left = left;
    private Node Right = right;

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "more than or equal: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
public class LessThanEqOperator(Node left, Node right) : Node
{
    private Node Left = left;
    private Node Right = right;

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "less than or equal: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
#endregion relational

#endregion non-math

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
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        try
        {
            PwInstance l_val = Left.Evaluate(scope);
            PwInstance r_val = Right.Evaluate(scope);

            Parser.Log($"Addition: {ToPrettyString(Level + 1)}");
            return l_val.GetMethod("__add__").Invoke(l_val, r_val);
        }
        catch (Exception exception)
        {
            Parser.Log($"Unable to parse expression: {Left} + {Right}: {exception.Message}. (Likely a type mismatch.)");
            return null;
        }
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "addition: (\r\n");
        s += AddSpaces(level + 1, $"left: \r\n" +
                                  $"{Left.ToPrettyString(level + 2)},\r\n");
        s += AddSpaces(level + 1, $"right: \r\n" +
                                  $"{Right.ToPrettyString(level + 2)}\r\n");
        s += AddSpaces(level, ")");
        return s;
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
    
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        try
        {
            PwInstance l_val = Left.Evaluate(scope);
            PwInstance r_val = Right.Evaluate(scope);
            Parser.Log($"Subtraction: {ToPrettyString(Level + 1)}");
            return l_val.GetMethod("__sub__").Invoke(l_val, r_val);
        }
        catch
        {
            throw new Exception($"Unable to parse expression: {Left.Evaluate(scope).ToString()} - {Right.Evaluate(scope).ToString()}. Expected integer values.");
        }
    }
    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "subtraction: (\r\n");
        s += AddSpaces(level + 1, $"left: \r\n" +
                                  $"{Left.ToPrettyString(level + 2) },\r\n");
        s += AddSpaces(level + 1, $"right: \r\n" +
                                  $"{Right.ToPrettyString(level + 2)}\r\n");
        s += AddSpaces(level, ")");
        return s;
    }
}
public class Negative(Node term) : Node
{
    public readonly Node Term = term;

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        PwInstance t = Term.Evaluate(scope);
        return t.GetMethod("__neg__").Invoke(t);
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "negation: (\r\n");
        AddSpaces(level, s);
        s += AddSpaces(level + 1, $"term: {Term.ToPrettyString(0)}\r\n"); 
        s += AddSpaces(level, ")");
        return s;
    }
}
public class Multiply(Node left, Node right) : Node
{
    public readonly Node Left = left;
    public readonly Node Right = right;

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        PwInstance l_val = Left.Evaluate(scope);
        PwInstance r_val = Right.Evaluate(scope);
        return l_val.GetMethod("__mult__").Invoke(l_val, r_val);
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "multiplication: (\r\n");
        s += AddSpaces(level + 1, $"left: \r\n" +
                                  $"{Left.ToPrettyString(level + 2) },\r\n");
        s += AddSpaces(level + 1, $"right: \r\n" +
                                  $"{Right.ToPrettyString(level + 2)}\r\n");
        s += AddSpaces(level, ")");
        return s;
    }
}
public class Divide(Node left, Node right) : Node
{
    public readonly Node Left = left;
    public readonly Node Right = right;

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        PwInstance l_val = Left.Evaluate(scope);
        PwInstance r_val = Right.Evaluate(scope);
        return l_val.GetMethod("__div__").Invoke(l_val, r_val);
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "division: (\r\n");
        s += AddSpaces(level + 1, $"left: \r\n" +
                                  $"{Left.ToPrettyString(level + 2) },\r\n");
        s += AddSpaces(level + 1, $"right: \r\n" +
                                  $"{Right.ToPrettyString(level + 2)}\r\n");
        s += AddSpaces(level, ")");
        return s;
    }
}
#endregion

#endregion operator
