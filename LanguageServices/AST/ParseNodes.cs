using System;
using System.Collections.Generic;
using System.Text;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;

internal class ExitNode : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        throw new PwExit();
    }

    public override string ToPrettyString(int level)
    {
        return AddSpaces(level, "simple expression: exit");
    }
    
}
internal class VoidNode : PwAst
{
    public override PwInstance Evaluate(PwScope scope) { return null; }
    public override string ToPrettyString(int level)
    {
        return AddSpaces(level, "void");
    }
}
