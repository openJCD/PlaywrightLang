using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;
public class Name(string identifier) : Node, IQualifiedIdentifier
{
    public readonly string Identifier = identifier;

    // change this to access some kind of stack!
    public override object Evaluate(ScopedSymbolTable scope)
    {
        Parser.Log($"Found Name: {Identifier}");

        return scope.Lookup(Identifier);
    }

    public void Set(object value)
    {
        throw new System.NotImplementedException();
    }

    public override string ToPrettyString(int level) => AddSpaces(level, $"name: '{Identifier}'");
}
public class Integer(int value) : Node
{
    public int Value { get; private set; } = value;

    public override object Evaluate(ScopedSymbolTable scope)
    {
        return new PwPrimitive("new_int", Value, scope);
    }

    public override string ToPrettyString(int level)
    {
        return AddSpaces(level , $"int: {Value.ToString()}");
    }
}
public class StringLit(string value) : Node
{
    public readonly string Value = value;

    public override object Evaluate(ScopedSymbolTable scope) => new PwPrimitive("new_string", Value, scope);

    public override string ToPrettyString(int level) => AddSpaces(level, $"string: \"{Value}\"");
}

public class FloatLit(float value) : Node
{
    public override object Evaluate(ScopedSymbolTable scope)
    {
        return value;
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, $"float: {value}");
        return s;
    }
}

public class BooleanLit(bool isTrue) : Node
{
    public override object Evaluate(ScopedSymbolTable scope)
    {
        return isTrue;
    }

    public override string ToPrettyString(int level) => AddSpaces(level, $"bool: {(isTrue ? "true" : "false")}");
}