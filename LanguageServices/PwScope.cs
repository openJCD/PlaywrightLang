#nullable enable
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using PlaywrightLang.LanguageServices.Object;

namespace PlaywrightLang.LanguageServices;

public class PwScope(string name, int level, PwScope? parent)
{
    private Dictionary<string, PwInstance> Symbols = new();
    public readonly PwScope Parent = parent;
    public readonly int Level = level;
    public readonly string Name = name;

    internal PwInstance Lookup(string id)
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

        if (parent != null && parent.HasSymbol(name))
        {
            parent.MutateSymbol(name, symbol);
        }
        else 
        {
            Symbols[name] = symbol;
        }
        
    }
    internal bool HasSymbol(string symbol)
    {
        if (Symbols.ContainsKey(symbol))
        {
            return true;
        }
        else
        {
            if (parent != null)
                return parent.HasSymbol(symbol);
            else
                return false;
        }
    }
    
    internal void AddSymbol(string name, PwInstance symbol)
    {
        Symbols.Add(name, symbol);
    }
}