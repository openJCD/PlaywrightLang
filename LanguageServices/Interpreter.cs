namespace PlaywrightLang.LanguageServices;

public class Interpreter(Node rootnode)
{
    readonly Node _root = rootnode;

    public void Execute()
    {
        _root.Evaluate();
    }
}