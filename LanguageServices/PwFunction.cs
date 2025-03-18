using PlaywrightLang.LanguageServices.AST;

namespace PlaywrightLang.LanguageServices;

public class PwFunction : PwObject
{
    // TODO: Add mechanism to create PwFunction from a C# Method
    public readonly Node[] Instructions;
    private string _callerType;
    private string[] argIdentifiers;
    public PwFunction(string id, string[] argIds, Node[] instructions, string callerType) : base()
    {
        Name = id;
        _callerType = callerType;
        Instructions = instructions;
        argIdentifiers = argIds;
    }

    public PwObject Invoke(PwObject caller, ScopedSymbolTable scope, PwObject[] args)
    {
        if (args.Length != argIdentifiers.Length)
        {
            throw new PwException($"Invalid number of arguments for function invocation {Name}");
        }
        else
        {
            // for each argument passed in, add it to the current nested scope as an object with
            // the name of the argument as its ID. 
            for (int i = 0; i < args.Length; i++)
            {
                scope.AddSymbolAlias(argIdentifiers[i], args[i]);
            }

            if (caller != null)
            { 
                // add the symbol 'self' to enable self-mutation and calls on own members, etc.
                scope.AddSymbolAlias("self", this);
            }
        }
        foreach (Node n in Instructions)
        {
            if (n is ReturnStmt)
            {
                return (PwObject)n.Evaluate(scope);
            }
            n.Evaluate(scope);
        }

        return null;
    }
}