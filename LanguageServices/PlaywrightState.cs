﻿using System;
using System.Collections.Generic;

namespace PlaywrightLang.LanguageServices;

public class PlaywrightState
{
    private Tokeniser tokeniser;
    private Parser parser;
    private Dictionary<string, PwObject> Globals = new();
    public PlaywrightState() { }

    List<Token> LoadTokenise(string path)
    {
        tokeniser = new Tokeniser(path);
        Console.WriteLine("Tokens: ");
        List<Token> tokens = tokeniser.Tokenise();
        tokens.ForEach(t => Console.WriteLine(t.Type.ToString() + " " + t.Value?.ToString()));
        return tokens;
    }

    public void Parse()
    {
        parser = new Parser(LoadTokenise("script.pw"));
        Node tree = parser.ParseExpression();
        Parser.Log(tree.Evaluate().ToString());
    }
}