using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace PlaywrightLang.LanguageServices;

public class PwState
{
    private Tokeniser tokeniser;
    private Parser parser;
    private Dictionary<string, PwObject> Globals = new();
    private Dictionary<string, List<PwObject>> ScopeTable = new();
    private string _currentScope = "global"; 
    private int _currentScopeIndex = 0;
    private Dictionary<string, PwActor> Cast = new();
    private Dictionary<string, Type> ValidActorTypes = new();
    private ScopedSymbolTable Scope = new("global", 0, null);
    public PwState()
    {
        ValidActorTypes.Add("actor", typeof(PwActor));
        ScopeTable.Add("global", []);
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
        timer.Stop();
        Parser.Log(new Interpreter(tree).Execute());
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
        return Scope.Lookup(name);
    }
    internal void SetActor(string name, string actorType)
    {
        if (!ValidActorTypes.ContainsKey(actorType))
        {
            throw new PwTypeException(actorType, "Type does not exist. Have you tried registering it with RegisterActorType<T>()?");
        }
        else
        {
            Cast[actorType] = new PwActor(name, this);
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

    internal PwFunction GetGlobalFunction(string name)
    {
        return Globals[name] as PwFunction;
    }
    
    internal void EnterNestedScope(string scopeName)
    {
        _currentScope = scopeName;
        _currentScopeIndex++;
        ScopeTable.Add(_currentScope, []);
    }

    internal void ExecuteScopedInstructions(Node instructions, Node[] args)
    {
        // TODO:  
    }

    internal object InvokeFunction(PwFunction func, params object[] args)
    {
        return func.Invoke(Scope, args);
    }

    internal void MoveOutOfCurrentScope()
    {
        _currentScopeIndex -= 1;
        ScopeTable.Remove(_currentScope);
        _currentScope = ScopeTable.Keys.ElementAt(_currentScopeIndex);
    }
}