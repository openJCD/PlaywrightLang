using System.Collections.Generic;

namespace PlaywrightLang.LanguageServices;

public class PlaywrightState
{
    private Tokeniser tokeniser;
    private Parser parser;
    private Dictionary<string, PwObject> Objects = new();
    public PlaywrightState() { }

    public List<Token> LoadTokenise(string path)
    {
        tokeniser = new Tokeniser(path);
        return tokeniser.Tokenise();
    }

    public void Parse()
    {
        parser = new Parser();
    }

    public T GetVariable<T>(string name)
    {
        Objects.TryGetValue(name, out PwObject value);
        return value.Get<T>();
    }
}