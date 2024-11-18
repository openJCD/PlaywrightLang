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

    internal void AddSymbol(PwObject symbol)
    {
        Symbols.Add(Name, symbol);
    }
}