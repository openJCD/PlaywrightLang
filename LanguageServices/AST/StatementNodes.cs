#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Security;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Object.Primitive;

namespace PlaywrightLang.LanguageServices.AST;


internal class Statement(PwAst expr) : PwAst 
{
    public override PwInstance Evaluate(PwScope scope)
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

internal class CompoundStmt(params Statement[] args) : PwAst
{
    private Statement[] statements = args;
    public override PwInstance Evaluate(PwScope scope)
    {
        foreach (Statement s in statements)
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

internal class ReturnStmt(PwAst retValue) :  PwAst
{
    public override PwInstance Evaluate(PwScope scope)
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

internal class Instantiation(string name, Name typeName, ParamExpressions args) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        var instance = typeName.Evaluate(scope).GetMethod("__new__").Invoke(args.AsArray(scope));
        scope.AddSymbol(name, instance);
        return instance;
    }
    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "instantiation: (\r\n");
        s += AddSpaces(level + 1, $"name: {name},\r\n");
        s += AddSpaces(level + 1, $"type: {typeName.Value},\r\n");
        s += args.ToPrettyString(level+1);
        s += AddSpaces(level, ")");
        return s;
    }
}

internal class FunctionBlock(string id, string owner, ParamNames args, CompoundStmt body) : PwAst
{
    private string Id = id;
    private string Owner = owner;
    private ParamNames Args = args;
    private CompoundStmt Body = body;
    public override PwInstance Evaluate(PwScope scope)
    {
        PwFunction pwFunction = new PwFunction(Args, Body, Owner, scope);
        PwCallableInstance callableFunction = new PwCallableInstance(pwFunction);
        scope.AddSymbol(Id, callableFunction);
        
        return callableFunction;
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

internal class IfStmt(Expression conditional, CompoundStmt statements, PwAst? elseStmt) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        if (conditional.IsTruthy(scope))
        {
            return statements.Evaluate(scope);
        }
        else
        {
            return elseStmt?.Evaluate(scope);
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

internal class WhileLoop(Expression conditional, CompoundStmt statements) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        while (conditional.IsTruthy(scope))
        {
            try
            {
                statements.Evaluate(scope);
            }
            catch (PwContinue)
            {
                continue;
            }
            catch (PwBreak)
            {
                break;
            }
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

internal class ForLoop(Name item, PwAst collection, CompoundStmt statements) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        PwScope local_scope = new PwScope("for_loop", scope.Level + 1, scope);
        
        PwInstance l_eval = collection.Evaluate(scope);
        if (!l_eval.HasMethod("__indg__") || !l_eval.HasMethod("__len__"))
        {
            throw new PwTypeException(l_eval.InstanceName,
                $"Collection {l_eval.InstanceName} either does not implement __indg__ or __len__ for indexing during iteration");
        }

        float length = (float)l_eval.GetMethod("__len__").Invoke().GetUnderlyingObject();
        int counter = 0;
        while (counter < length)
        {
            local_scope.MutateSymbol(item.Value, l_eval.GetMethod("__indg__").Invoke(counter));
            try
            {
                statements.Evaluate(local_scope);
            }
            catch (PwContinue)
            {
                // continues regardless
            }
            catch (PwBreak)
            {
                break;
            }

            counter++;
        }

        return new PwNullInstance();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "for: (\r\n");
        s += item.ToPrettyString(level+1) + " in:\r\n";
        s += collection.ToPrettyString(level + 1) + "\r\n";
        s += statements.ToPrettyString(level + 1) + "\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

internal class ContinueStmt() : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        throw new PwContinue();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "continue");
        return s;
    }
}

internal class BreakStmt() : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        throw new PwBreak();
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, "break");
        return s;
    }
}