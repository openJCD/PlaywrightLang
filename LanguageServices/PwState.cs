using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
        Console.WriteLine("> Playwright State: " + message);
    }
    
    public Tokeniser Tokeniser { get; private set; }
    private Parser parser;
    private PwScope CurrentScope;
    private PwDefaults _defaults;
    public PwState()
    {
        _defaults = new PwDefaults(this);
        RegisterDefaults();
    }

    private void RegisterDefaults()
    {
        CurrentScope = new("global", 0, null);
        RegisterType<PwActor>("actor");
        RegisterType<PwNumeric>("float");
        RegisterType<PwString>("string");
        RegisterType<PwObjectClass>("object");
        RegisterType<PwFunction>("function");
        RegisterGlobalFunctions(_defaults);
    }
    public List<Token> LoadFile(string path, bool verbose = false)
    {
        Stream s = File.OpenRead(path);
        StreamReader sr = new StreamReader(s);
        Tokeniser = new Tokeniser(sr.ReadToEnd());
        List<Token> tokens = Tokeniser.Tokenise();
        if (verbose)
        {
            Console.WriteLine($"Playwright Lexer: File {path} -> Tokens: ");
            int count = 0;
            tokens.ForEach(t =>
            {
                Console.WriteLine($" {count} - {t.Type} {t.Value}");
                count++;
            });
        }
        return tokens;
    }

    List<Token> LoadString(string s, bool verbose = false)
    {
        Tokeniser = new Tokeniser(s);
        List<Token> tokens = Tokeniser.Tokenise();
        if (verbose)
        {
            Console.WriteLine("• Playwright Lexer: String Input -> Tokens: ");
            int count = 0;
            tokens.ForEach(t =>
            {
                Console.WriteLine($" {count} - {t.Type} {t.Value}");
                count++;
            });
        }
        return tokens;
    }
    
    public PwAst ParseString(string input, bool verbose = false)
    { 
        PwAst tree = parser.Parse();
        if (verbose) Console.Write(tree.ToPrettyString(0));
        return tree;
    }

    public PwAst ParseFile(string filepath, bool verbose = false)
    {
        parser = new Parser(LoadFile(filepath, verbose));
        Stopwatch timer = Stopwatch.StartNew();
        PwAst tree = parser.Parse();
        timer.Stop();
        if (verbose)
        {
            Parser.Log($"Done in {timer.ElapsedMilliseconds}ms");
            Parser.Log(tree.ToPrettyString(0) ?? string.Empty);
        }
        return tree;
    }

    /// <summary>
    /// Resets the program state (scope and variables), and executes the given AST.
    /// </summary>
    /// <param name="node">Abstract Syntax Tree top-level node to execute.</param>
    /// <returns></returns>
    public object ExecuteChunk(PwAst node)
    {
        RegisterDefaults();
        return node.Evaluate(CurrentScope).GetUnderlyingObject();
    }

    public object ExecuteFile(string path)
    {
        var tokens = LoadFile(path);
        var node = ParseFile(path);
        return ExecuteChunk(node);
    }

    public object ExecuteString(string input)
    {
        var tokens = LoadString(input);
        var node = ParseString(input);
        return ExecuteChunk(node);
    }
    
    public object ExecuteFunction(string funcName, params object[] args)
    {
        if (CurrentScope.Lookup(funcName) is PwCallableInstance callable)
        {
            PwInstance[] pwArgs = new PwInstance[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                pwArgs[i] = args[i].AsPwInstance();
            }

            return callable.Invoke(pwArgs);
        }

        return null;
    }
    public void RegisterType<T>(string typeName) where T : PwObjectClass
    {
        CurrentScope.AddSymbol(typeName, new PwCsharpInstance(new PwType<T>(typeName)));
    }

    public void RegisterFunction(string funcName, object target, MethodInfo methodInfo)
    {
        PwCsharpCallable innerCallable = new PwCsharpCallable(methodInfo, target);
        PwCallableInstance instance = new PwCallableInstance(innerCallable);
        CurrentScope.AddSymbol(funcName, instance);
    }

    /// <summary>
    /// Method that will iterate through all (including static and non-public)  members of a class and place any methods with PwItemAttribute into the global scope.
    /// </summary>
    public void RegisterGlobalFunctions(object target)
    {
        foreach (MethodInfo methodInfo in target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                                                      BindingFlags.Public | BindingFlags.NonPublic))
        {
            PwItemAttribute attr = methodInfo.GetCustomAttribute<PwItemAttribute>();
            if (attr != null)
                RegisterFunction(attr.PwName, target, methodInfo);
        }
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