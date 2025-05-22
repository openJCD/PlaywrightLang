using System;
using System.Linq;
using PlaywrightLang.LanguageServices.AST;
using PlaywrightLang.LanguageServices.Object;

namespace PlaywrightLang.LanguageServices;

public class PwFunction : PwCallable
{
    public readonly CompoundStmt Instructions;
    private string _callerType;
    private ParamNames _parameters;
    private ScopedSymbolTable _capturedScope;
    
    public PwFunction(ParamNames parameters, CompoundStmt body, string callerType, ScopedSymbolTable scope) : base()
    {
        _callerType = callerType;
        Instructions = body;
        _parameters = parameters;
        _capturedScope = scope;
    }

    public override PwInstance Invoke(PwInstance[] args)
    {
        //TODO: Add args to a new layer of the captured scope. 
        int i = 0;
        ScopedSymbolTable scope = new ScopedSymbolTable("func", _capturedScope.Level + 1, _capturedScope);
        foreach (var arg in args)
        {
            var paramDict = _parameters.GetParameters(scope);
            scope.MutateSymbol(paramDict.Keys.ToArray()[i], arg); // for each argument, add its literal to the 
            i++;
        }
        try
        {
            Instructions.Evaluate(scope);
        }
        catch (PwReturn r)
        {
            return r.ReturnValue;
        }

        // TODO: Replace with Playwright Null value, if practical to implement such a thing.
        return new PwNullInstance();
    }
}

public class PwReturn(PwInstance i) : Exception
{
    public readonly PwInstance ReturnValue = i; 
}