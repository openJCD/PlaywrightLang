#nullable enable
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Object.Primitive;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;

internal class Expression(PwAst expr) : PwAst
{
    PwAst Inner = expr;
    public override PwInstance Evaluate(PwScope scope)
    {
        return expr.Evaluate(scope);
    }

    internal bool IsTruthy(PwScope scope)
    {
        PwInstance result = expr.Evaluate(scope);
        return (bool)result.GetMethod("__true__").Invoke().GetUnderlyingObject();
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

internal class FunctionCall(PwAst left, ParamExpressions args) : PwAst
{
    internal PwAst Left = left;
    
    public override PwInstance Evaluate(PwScope scope)
    {
        var argArr = args.AsArray(scope);
        return (Left.Evaluate(scope) as PwCallableInstance).Invoke(argArr);
    }
    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, $"function call: (\r\n");
        s += $"{Left.ToPrettyString(level + 1)},\r\n";
        s += $"{args.ToPrettyString(level + 1)}\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

internal class ParamExpressions(ParamExpressions previous, PwAst current) : PwAst
{

    ParamExpressions Previous = previous;
    PwAst Current = current;
    public override PwInstance Evaluate(PwScope scope)
    {
        return null;
    }

    internal PwInstance[] AsArray(PwScope scope)
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
        if (current is not VoidNode)
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

internal class ParamNames(ParamNames previous, DeclarationParameter current) : PwAst
{
    private ParamNames Prev = previous;
    private DeclarationParameter Current = current;
    public override PwInstance Evaluate(PwScope scope)
    {
        return null;
    }
    // compile a dictionary of the prior ParamNames plus the current one.
    // the PwInstance? in each pair represents the default value 
    internal Dictionary<string, PwInstance?> GetParameters(PwScope scope)
    {
        Dictionary<string, PwInstance?> parameters;
        if (Prev != null)
        {
             parameters = previous.GetParameters(scope);
        }
        else
        {
            parameters = new();
        }

        if (current != null)
        {
            parameters[Current.Id] = Current.Evaluate(scope);
        }
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

internal class DeclarationParameter(Name id, PwAst? literal) : PwAst
{
    internal string Id = id.Value;
    private PwAst? Literal = literal;
    
    public override PwInstance Evaluate(PwScope scope)
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

internal class AssignmentExpression(IQualifiedIdentifier lvalue, PwAst rvalue) : PwAst 
{
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance r_eval = rvalue.Evaluate(scope);
        r_eval.InstanceName = lvalue.GetLastName();
        lvalue.Set(r_eval, scope);
        return r_eval;
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

internal class IncrementalAssignment(IQualifiedIdentifier lvalue, PwAst rvalue) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance r_eval = rvalue.Evaluate(scope);
        PwInstance l_eval = lvalue.Evaluate(scope);
        PwInstance result = l_eval.GetMethod("__add__").Invoke(l_eval, r_eval);
        lvalue.Set(r_eval, scope);
        return r_eval;    
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
internal class DecrementalAssignment(IQualifiedIdentifier lvalue, PwAst rvalue) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance r_eval = rvalue.Evaluate(scope);
        PwInstance l_eval = lvalue.Evaluate(scope);
        PwInstance result = l_eval.GetMethod("__sub__").Invoke(l_eval, r_eval);
        lvalue.Set(r_eval, scope);
        return r_eval;        
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

internal class DivAssignment(IQualifiedIdentifier lvalue, PwAst rvalue) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance r_eval = rvalue.Evaluate(scope);
        PwInstance l_eval = lvalue.Evaluate(scope);
        PwInstance result = l_eval.GetMethod("__div__").Invoke(l_eval, r_eval);
        lvalue.Set(r_eval, scope);
        return r_eval;
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
internal class MultAssignment(IQualifiedIdentifier lvalue, PwAst rvalue) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance r_eval = rvalue.Evaluate(scope);
        PwInstance l_eval = lvalue.Evaluate(scope);
        PwInstance result = l_eval.GetMethod("__mul__").Invoke(l_eval, r_eval);
        lvalue.Set(r_eval, scope);
        return r_eval;
        
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

internal class LogicalOr(PwAst lvalue, PwAst rvalue) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_eval = lvalue.Evaluate(scope);
        PwInstance r_eval = rvalue.Evaluate(scope);
        bool l_true = (bool)l_eval.GetMethod("__true__").Invoke().GetUnderlyingObject();
        bool r_true = (bool)l_eval.GetMethod("__true__").Invoke().GetUnderlyingObject();
        return (l_true || r_true).AsPwInstance();    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "or expression: (\r\n");
        s += $"{lvalue.ToPrettyString(level + 1) },\r\n";
        s += $"{rvalue.ToPrettyString(level + 1)}\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

internal class LogicalAnd(PwAst lvalue, PwAst rvalue) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_eval = lvalue.Evaluate(scope);
        PwInstance r_eval = rvalue.Evaluate(scope);
        bool l_true = (bool)l_eval.GetMethod("__true__").Invoke().GetUnderlyingObject();
        bool r_true = (bool)l_eval.GetMethod("__true__").Invoke().GetUnderlyingObject();
        return (l_true && r_true).AsPwInstance();
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

internal class UnaryNot(PwAst value) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        return value.Evaluate(scope).GetMethod("__not__").Invoke();
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
internal class AccessOperator(PwAst left, Name right) : PwAst, IQualifiedIdentifier
{
    private PwAst Left = left;
    private Name Right = right;
    private PwInstance l_eval = null;
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance left = Left.Evaluate(scope);

        return left.Get(Right.Value); 
    }

    public void Set(PwInstance obj, PwScope scope)
    {
        if (l_eval == null)
        {
            l_eval = Left.Evaluate(scope);
        }
        l_eval.Set(Right.Value, obj);
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "access member: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }

    public string GetLastName()
    {
        return Right.Value;
    }
}

#region equality
internal class EqualOperator(PwAst left, PwAst right) : PwAst
{
    private PwAst Left = left;
    private PwAst Right = right;
    
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_eval = Left.Evaluate(scope);
        PwInstance r_eval = Right.Evaluate(scope);
        PwCallableInstance left_eq = l_eval.GetMethod("__eq__");
        return left_eq.Invoke(l_eval, r_eval);
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

internal class NotEqualOperator(PwAst left, PwAst right) : PwAst
{
    private PwAst Left = left;
    private PwAst Right = right;

    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_eval = Left.Evaluate(scope);
        PwInstance r_eval = Right.Evaluate(scope);
        PwCallableInstance left_eq = l_eval.GetMethod("__neq__");
        return left_eq.Invoke(l_eval, r_eval);
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

internal class GreaterThanOperator(PwAst left, PwAst right) : PwAst
{
    private PwAst Left = left;
    private PwAst Right = right;

    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_eval = Left.Evaluate(scope);
        PwInstance r_eval = Right.Evaluate(scope);
        PwCallableInstance left_eq = l_eval.GetMethod("__gt__");
        return left_eq.Invoke(l_eval, r_eval);
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
internal class LessThanOperator(PwAst left, PwAst right) : PwAst
{
    private PwAst Left = left;
    private PwAst Right = right;

    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_eval = Left.Evaluate(scope);
        PwInstance r_eval = Right.Evaluate(scope);
        PwCallableInstance left_eq = l_eval.GetMethod("__lt__");
        return left_eq.Invoke(l_eval, r_eval);    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "less than: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
internal class GreaterThanEqOperator(PwAst left, PwAst right) : PwAst
{
    private PwAst Left = left;
    private PwAst Right = right;

    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_eval = Left.Evaluate(scope);
        PwInstance r_eval = Right.Evaluate(scope);
        PwCallableInstance left_eq = l_eval.GetMethod("__geq__");
        return left_eq.Invoke(l_eval, r_eval);    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "greater or equal: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
internal class LessThanEqOperator(PwAst left, PwAst right) : PwAst
{
    private PwAst Left = left;
    private PwAst Right = right;

    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_eval = Left.Evaluate(scope);
        PwInstance r_eval = Right.Evaluate(scope);
        PwCallableInstance left_eq = l_eval.GetMethod("__leq__");
        return left_eq.Invoke(l_eval, r_eval);    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "less or equal: (\r\n");
        s += $"{left.ToPrettyString(level + 1) },\r\n";
        s += $"{right.ToPrettyString(level + 1) }\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}
#endregion relational

#endregion non-math

#region math
internal class Add : PwAst
{
    internal readonly PwAst Left;
    
    internal readonly PwAst Right;

    internal Add(PwAst left, PwAst right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_val = null;
        PwInstance r_val = null;
        
        l_val = Left.Evaluate(scope);
        r_val = Right.Evaluate(scope);

        return l_val.GetMethod("__add__").Invoke(l_val, r_val);
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
internal class Subtract : PwAst
{
    internal readonly PwAst Left;
    internal readonly PwAst Right;
    internal Subtract(PwAst left, PwAst right)
    {
        this.Left = left;
        this.Right = right;
    }
    
    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_val = Left.Evaluate(scope);
        PwInstance r_val = Right.Evaluate(scope);
        return l_val.GetMethod("__sub__").Invoke(l_val, r_val);
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
internal class Negative(PwAst term) : PwAst
{
    internal readonly PwAst Term = term;

    public override PwInstance Evaluate(PwScope scope)
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
internal class Multiply(PwAst left, PwAst right) : PwAst
{
    internal readonly PwAst Left = left;
    internal readonly PwAst Right = right;

    public override PwInstance Evaluate(PwScope scope)
    {
        PwInstance l_val = Left.Evaluate(scope);
        PwInstance r_val = Right.Evaluate(scope);
        return l_val.GetMethod("__mul__").Invoke(l_val, r_val);
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
internal class Divide(PwAst left, PwAst right) : PwAst
{
    internal readonly PwAst Left = left;
    internal readonly PwAst Right = right;

    public override PwInstance Evaluate(PwScope scope)
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
