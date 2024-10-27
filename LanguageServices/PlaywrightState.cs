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
    private Dictionary<string, PwActor> Cast = new();
    private Dictionary<string, Type> ValidActorTypes = new();

    public PlaywrightState()
    {
        ValidActorTypes.Add("actor", typeof(PwActor));
    }

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
        Node tree = parser.ParseChunk();
        Parser.Log($"Done in {timer.ElapsedMilliseconds}ms");
        new Interpreter(tree).Execute();
    }

    public void RegisterActorType<T>(string identifier)
    {
        ValidActorTypes[identifier] = typeof(T);
    }

    internal void SetVariable(string name, PwObject value)
    {
        Globals[name] = value;
    }

    internal object GetVariable(string name)
    {
        return Globals[name];
    }
    internal void SetActor(string name, string actorType)
    {
        if (!ValidActorTypes.ContainsKey(actorType))
        {
            throw new PwTypeException(actorType, "Type does not exist. Have you tried registering it with RegisterActorType<T>()?");
        }
        else
        {
            Cast[actorType] = new PwActor(name);
        }
    }

    internal PwActor GetActor(string name)
    {
        try
        {
            return Cast[name];
        }
        catch (Exception e)
        {
            throw new PwException($"Could not find actor {name}", e);
        }
    }

    internal PwFunction GetFunction(string name)
    {
        return Globals[name] as PwFunction;
    }
    
    internal object RunMethod(PwActor actor, PwFunction fn, params object[] args)
    {
        if (actor != null) return fn.Call(actor, args);
        else return null;
    }
}