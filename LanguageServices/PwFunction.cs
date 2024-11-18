namespace PlaywrightLang.LanguageServices;

public class PwFunction : PwObject
{
    public readonly Node[] Instructions;
    private string caller;
    public PwFunction(string id, Node[] instructions, string actor) : base()
    {
        Name = id;
        caller = actor;
        Data = $"{Name}: {instructions}";
        Instructions = instructions;
    }

    public object Invoke(ScopedSymbolTable scope, params object[] args)
    {
        foreach (Node n in Instructions)
        {
            if (n is ReturnStmt)
            {
                return n.Evaluate();
            }
            n.Evaluate();
        }

        return null;
    }
}