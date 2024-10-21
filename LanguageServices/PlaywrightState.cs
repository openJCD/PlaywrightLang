using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        int count = 0;
        tokens.ForEach(t =>
        {
            Console.WriteLine($" {count} - {t.Type} {t.Value}");
            count++;
        });
        return tokens;
    }

    List<Token> LoadString(string s)
    {
        tokeniser = new Tokeniser(s);
        Console.WriteLine("• Playwright Lexer: String Input -> Tokens: ");
        List<Token> tokens = tokeniser.Tokenise();
        int count = 0;
        tokens.ForEach(t =>
        {
            Console.WriteLine($" {count} - {t.Type} {t.Value}");
            count++;
        });
        return tokens;
    }
    
    public void ParseString(string input)
    {
        parser = new Parser(LoadString(input), this);
        parser.ParseChunk();
    }

    public void ParseFile(string filepath)
    {
        parser = new Parser(LoadFile(filepath), this);
        Stopwatch timer = Stopwatch.StartNew();
        parser.ParseChunk();
        Parser.Log($"Done in {timer.ElapsedMilliseconds}ms");
    }

    internal void SetVariable(string name, PwObject value)
    {
        if (!Globals.ContainsKey(name))
            Globals.Add(name, value);
        else
            Globals[name] = value;
    }
}