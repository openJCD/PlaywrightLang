using System;
using System.Collections.Generic;
using System.IO;

namespace PlaywrightLang.LanguageServices;

public class PlaywrightState
{
    private Tokeniser tokeniser;
    private Parser parser;
    private Dictionary<string, PwObject> Globals = new();
    public PlaywrightState() { }

    List<Token> LoadFile(string path)
    {
        Stream s = File.OpenRead(path);
        StreamReader sr = new StreamReader(s);
        tokeniser = new Tokeniser(sr.ReadToEnd());
        Console.WriteLine($"Playwright Lexer: File {path} -> Tokens: ");
        List<Token> tokens = tokeniser.Tokenise();
        tokens.ForEach(t => Console.WriteLine($" - {t.Type} {t.Value}"));
        return tokens;
    }

    List<Token> LoadString(string s)
    {
        tokeniser = new Tokeniser(s);
        Console.WriteLine("• Playwright Lexer: String Input -> Tokens: ");
        List<Token> tokens = tokeniser.Tokenise();
        tokens.ForEach(t => Console.WriteLine($" - {t.Type} {t.Value}"));
        return tokens;
    }
    
    public Node ParseString(string input)
    {
        parser = new Parser(LoadString(input));
        Node tree = parser.ParseExpression();
        string result = tree.ToString();
        Parser.Log(result);
        return tree;
    }

    public Node ParseFile(string filepath)
    {
        parser = new Parser(LoadFile(filepath));
        Node tree = parser.ParseExpression();
        Parser.Log(tree.Evaluate().ToString());
        return tree;
    }
}