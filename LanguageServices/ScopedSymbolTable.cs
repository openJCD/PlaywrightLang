#nullable enable
using System.Collections.Generic;
using PlaywrightLang.LanguageServices.Object;

namespace PlaywrightLang.LanguageServices;

public class ScopedSymbolTable(string name, int level, ScopedSymbolTable? parent)
{
    private Dictionary<string, PwInstance> Symbols = new();
    public readonly ScopedSymbolTable Parent = parent;
    public readonly int Level = level;
    public readonly string Name = name;

    public PwInstance Lookup(string id)
    {
        if (Symbols.Keys.Contains(id))
        {
            return Symbols[id];
        }
        if (Parent != null) return Parent.Lookup(id);
        throw new PwException($"No symbol found for '{id}' in scope '{Name}'.");
    }
    internal void MutateSymbol(string name, PwInstance symbol)
    {
        Symbols[name] = symbol;
    }
    
    internal void AddSymbol(string name, PwInstance symbol)
    {
        Symbols.Add(name, symbol);
    }

    internal void AddSymbolAlias(string alias, PwInstance symbol)
    {
        Symbols.Add(alias, symbol);
    }
}