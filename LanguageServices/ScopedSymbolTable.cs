#nullable enable
using System.Collections.Generic;

namespace PlaywrightLang.LanguageServices;

public class ScopedSymbolTable(string name, int level, ScopedSymbolTable? parent)
{
    private Dictionary<string, PwObject> Symbols = new();
    public readonly ScopedSymbolTable Parent = parent;
    public readonly int Level = level;
    public readonly string Name = name;

    public PwObject Lookup(string id)
    {
        if (Symbols.Keys.Contains(id))
        {
            return Symbols[id];
        }
        if (Parent != null) return Parent.Lookup(id);
        throw new PwException($"No symbol found for '{id}' in scope '{Name}'.");
    }

    public PwObject LocalLookup(string id)
    {
        if (Symbols.Keys.Contains(id))
        {
            return Symbols[id];
        }
        else
        {
            throw new PwException($"No symbol found for '{id}' in scope '{Name}'.");
        }
    }
    
    internal void MutateSymbol(string name, PwObject symbol)
    {
        Symbols[name] = symbol;
    }
    
    internal void AddSymbol(PwObject symbol)
    {
        Symbols.Add(symbol.Name, symbol);
    }

    internal void AddSymbolAlias(string alias, PwObject symbol)
    {
        Symbols.Add(alias, symbol);
    }
}