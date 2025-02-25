using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Transactions;
using System.Xml.Schema;

namespace PlaywrightLang.LanguageServices;

public class PwState
{
    public static void Log(string message)
    {
        Console.WriteLine("- Playwright State: " + message);
    }
    
    private string _currentScopeName = "global"; 
    private int _currentScopeIndex = 0;
    
    private Tokeniser tokeniser;
    private Parser parser;
    
    private Dictionary<string, PwObject> Globals = new();
    private Dictionary<string, Type> ValidActorTypes = new();
    private Dictionary<string, List<PwActor>> ActorTypeLookup = new();
    private ScopedSymbolTable CurrentScope = new("global", 0, null);
    
    public PwState()
    {
        RegisterActorType<PwActor>("actor");
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
        Parser.Log(tree.ToString() ?? string.Empty);
    }

    public object ExecuteChunk(Node node)
    {
        return node.Evaluate(CurrentScope);
    }
    public void RegisterActorType<T>(string identifier)
    {
        ValidActorTypes[identifier] = typeof(T);
        ActorTypeLookup[identifier] = new List<PwActor>();
    }

    internal void SetGlobal(string name, PwObject value)
    {
        Globals[name] = value;
    }
    internal void SetActor(string name, string actorType)
    {
        if (!ValidActorTypes.ContainsKey(actorType))
        {
            throw new PwTypeException(actorType, "Type does not exist. Have you tried registering it with RegisterActorType<T>()?");
        }
        else
        {
            PwActor actor = new PwActor(name, this, CurrentScope);
            ActorTypeLookup[actorType].Add(actor);
            actor.CacheAllInternalMethods();
            actor.RegisterAllDataMembers();
            CurrentScope.AddSymbol(actor);
        }
    }

    internal PwActor GetActor(string name)
    {
        try
        {
            return CurrentScope.Lookup(name) as PwActor;
        }
        catch (Exception e)
        {
            throw new PwException($"Could not find actor {name}", e);
        }
    }

    internal void RegisterPwFunction(PwFunction func, string actorType)
    {
        if (actorType == "all")
        {
            CurrentScope.AddSymbol(func);
        }
        else
        {
            foreach (var actor in ActorTypeLookup[actorType])
                actor.AddMethod(func.Name, func);
        }
    } 
    
    internal void EnterNestedScope(string scopeName)
    {
        _currentScopeName = scopeName;
        ScopedSymbolTable newScope = new ScopedSymbolTable(scopeName, _currentScopeIndex+1, CurrentScope);
        CurrentScope = newScope;
        _currentScopeIndex++;
    }

    internal object InvokeFunction(PwActor actor, PwFunction func, PwObject[] args)
    {
        EnterNestedScope(func.Name);
        object value = func.Invoke(actor, CurrentScope, args);
        MoveOutOfCurrentScope();
        return value;
    }

    internal object InvokeFunction(string funcName, PwObject[] args)
    {
        EnterNestedScope(funcName);
        PwFunction func = CurrentScope.Lookup(funcName) as PwFunction;
        if (func == null) throw new PwException($"Could not find function {funcName} in scope {CurrentScope.Name}");
        object value = func.Invoke(null, CurrentScope, args);
        MoveOutOfCurrentScope();
        return value;
    }
    internal void MoveOutOfCurrentScope()
    {
        _currentScopeIndex -= 1;
        if (CurrentScope.Name != "global")
        {
            CurrentScope = CurrentScope.Parent;
        }
        _currentScopeName = CurrentScope.Name;
    }
}