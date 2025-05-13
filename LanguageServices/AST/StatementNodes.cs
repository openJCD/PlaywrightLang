using System;
using System.Collections.Generic;
using System.Net.Security;
using PlaywrightLang.LanguageServices.Object;

namespace PlaywrightLang.LanguageServices.AST;


public class Statement(Node expr) : Node 
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        return expr.Evaluate(scope);
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "statement: (\r\n");
        s+= $"{expr.ToPrettyString(level + 1)}\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class CompoundStmt(params Statement[] args) : Node
{
    private Statement[] statements = args;
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        foreach (Statement s in args)
        {
            s.Evaluate(scope);
        }

        //TODO: Create null instance type.
        return new PwInstance();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "compound statement: (\r\n");
        foreach (var n in args)
        {
            s += $"{n.ToPrettyString(level + 1)},\r\n";
        }
        s += AddSpaces(level, ")");
        return s;
    }
}

public class ReturnStmt(Node retValue) :  Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new PwReturn(retValue.Evaluate(scope));
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "return: (\r\n");
        
        s += AddSpaces(level + 1, $"with: \r\n" +
                                  $"{retValue.ToPrettyString(level + 1)},\r\n");
        s += AddSpaces(level, ")");
        return s;
    }
}

public class Instantiation(string name, Name typeName, ParamExpressions args) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        var instance = typeName.Evaluate(scope).GetMethod("__new__").Invoke(args.AsArray(scope));
        scope.AddSymbol(name, instance);
        return instance;
    }
    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "instantiation: (\r\n");
        s += AddSpaces(level + 1, $"name: {name},\r\n");
        s += AddSpaces(level + 1, $"type: {typeName},\r\n");
        s += args.ToPrettyString(level+1);
        s += AddSpaces(level, ")");
        return s;
    }
}

public class FunctionBlock(string id, string owner, ParamNames args, CompoundStmt body) : Node
{
    private string Id = id;
    private string Owner = owner;
    private ParamNames Args = args;
    private CompoundStmt Body = body;
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, $"define function '{id}' for '{owner}': (\r\n");
        s += $"{args.ToPrettyString(level+1)},\r\n";
        s += $"{body.ToPrettyString(level + 1)},\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class IfStmt(Expression conditional, CompoundStmt statements) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        if (conditional.IsTruthy(scope))
        {
            return statements.Evaluate(scope);
        }
        
        return null;
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "if statement: ( \r\n");
        s += AddSpaces(level + 1, "condition: (");
        s += $"{conditional.ToPrettyString(level + 2)},\r\n";
        s += AddSpaces(level + 1, ")\r\n");
        s += $"{statements.ToPrettyString(level + 1)},\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class WhileLoop(Expression conditional, CompoundStmt statements) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        while (conditional.IsTruthy(scope))
        {
            statements.Evaluate(scope);
        }
        return null;
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "while loop: ( \r\n");
        s += AddSpaces(level + 1, "condition: (\r\n");
        s += $"{conditional.ToPrettyString(level + 2)},\r\n";
        s += AddSpaces(level + 1, ")\r\n");
        s += $"{statements.ToPrettyString(level + 1)},\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}