using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Transactions;
using System.Xml.Schema;
using PlaywrightLang.LanguageServices.AST;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Object.Primitive;
using PlaywrightLang.LanguageServices.Object.Types;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices;

public class PwState
{
    public static void Log(string message)
    {
        Console.WriteLine("- Playwright State: " + message);
    }
    
    private string _currentScopeName = "global"; 
    private int _currentScopeIndex = 0;
    public Tokeniser Tokeniser { get; private set; }
    private Parser parser;
    private ScopedSymbolTable CurrentScope = new("global", 0, null);
    
    public PwState()
    {
        RegisterType<PwActor>("actor");
        RegisterType<PwNumeric>("float");
        RegisterType<PwString>("string");
        RegisterType<PwObjectClass>("object");
        RegisterType<PwFunction>("function");
        // testing instantiation
        CreateInstanceOfTypeName("actor", "ronnie", "ronnie");
    }

    public List<Token> LoadFile(string path)
    {
        Stream s = File.OpenRead(path);
        StreamReader sr = new StreamReader(s);
        Tokeniser = new Tokeniser(sr.ReadToEnd());
        Console.WriteLine($"Playwright Lexer: File {path} -> Tokens: ");
        List<Token> tokens = Tokeniser.Tokenise();
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
        Tokeniser = new Tokeniser(s);
        Console.WriteLine("• Playwright Lexer: String Input -> Tokens: ");
        List<Token> tokens = Tokeniser.Tokenise();
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
        Node tree = parser.Parse();
        Console.Write(tree.ToPrettyString(0));
    }

    public void ParseFile(string filepath)
    {
        parser = new Parser(LoadFile(filepath));
        Stopwatch timer = Stopwatch.StartNew();
        Node tree = parser.Parse();
        timer.Stop();
        Parser.Log($"Done in {timer.ElapsedMilliseconds}ms");
        Parser.Log(tree.ToPrettyString(0) ?? string.Empty);
    }

    public object ExecuteChunk(Node node)
    {
        return node.Evaluate(CurrentScope);
    }

    public void RegisterType<T>(string typeName) where T : PwObjectClass
    {
        CurrentScope.AddSymbol(typeName, new PwCsharpInstance(new PwType<T>(typeName)));
    }
    
    internal void EnterNestedScope(string scopeName)
    {
        _currentScopeName = scopeName;
        ScopedSymbolTable newScope = new ScopedSymbolTable(scopeName, _currentScopeIndex+1, CurrentScope);
        CurrentScope = newScope;
        _currentScopeIndex++;
    }
    
    internal object InvokeFunction(string funcName, params PwInstance[] args)
    {
        EnterNestedScope(funcName);
        PwCallableInstance func = CurrentScope.Lookup(funcName) as PwCallableInstance;
        if (func == null) throw new PwException($"Could not find function {funcName} in scope {CurrentScope.Name}");
        object value = func.Invoke(args);
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

    private PwInstance CreateInstanceOfTypeName(string typeName, string id, params object[] args)
    {
        PwInstance[] pwArgs = new PwInstance[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            pwArgs[i] = args[i].AsPwInstance();
        }
        PwCallableInstance method =  CurrentScope.Lookup(typeName).GetMethod("__new__");
        PwInstance inst = method.Invoke(pwArgs);
        CurrentScope.AddSymbol(id, inst);
        return inst;
    }
    
}