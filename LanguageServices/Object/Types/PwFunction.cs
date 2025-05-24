using System.Linq;
using PlaywrightLang.LanguageServices.AST;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Object.Primitive;

namespace PlaywrightLang.LanguageServices;

internal class PwFunction : PwCallable
{
    public readonly CompoundStmt Instructions;
    private string _callerType;
    private ParamNames _parameters;
    private PwScope _capturedScope;
    
    internal PwFunction(ParamNames parameters, CompoundStmt body, string callerType, PwScope scope) : base()
    {
        _callerType = callerType;
        Instructions = body;
        _parameters = parameters;
        _capturedScope = scope;
    }

    public override PwInstance Invoke(object[] args)
    {
        //Add args to a new layer of the captured scope. 
        int i = 0;
        PwScope scope = new PwScope("func", _capturedScope.Level + 1, _capturedScope);
        foreach (var arg in args)
        {
            var paramDict = _parameters.GetParameters(scope);
            scope.MutateSymbol(paramDict.Keys.ToArray()[i], arg.AsPwInstance()); // for each argument, add its literal to the 
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

        return new PwNullInstance();
    }
}