namespace PlaywrightLang.LanguageServices;

public class Interpreter(Node rootnode)
{
    public string Execute()
    {
        return rootnode.Evaluate().ToString();
    }
}