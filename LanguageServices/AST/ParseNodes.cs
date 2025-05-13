using System;
using System.Collections.Generic;
using System.Text;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;

public class ExitNode : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
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
    public override PwInstance Evaluate(ScopedSymbolTable scope) { return null; }
    public override string ToPrettyString(int level)
    {
        return AddSpaces(level, "void");
    }
}

public class PostfixChain(Node[] operators) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
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

