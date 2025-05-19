using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Object.Primitive;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;

public interface IAtomNode
{ 
    public PwObjectClass AsPwObject(ScopedSymbolTable scope);
}

public class Name(string identifier) : Node, IQualifiedIdentifier
{
    public string Value { get; set; } = identifier;

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        Parser.Log($"Found Name: {Value}");

        return scope.Lookup(Value);
    }

    public void Set(object value)
    {
        throw new System.NotImplementedException();
    }

    public override string ToPrettyString(int level) => AddSpaces(level, $"name: '{Value}'");
}
public class Integer(int value) : Node, IAtomNode
{
    public int Value { get; private set; } = value;

    public override PwInstance Evaluate(ScopedSymbolTable scope) => new PwCsharpInstance(new PwNumeric(Value));
    public PwObjectClass AsPwObject(ScopedSymbolTable scope)
    {
        return new PwNumeric(Value);
    }

    public override string ToPrettyString(int level)
    {
        return AddSpaces(level , $"int: {Value.ToString()}");
    }
}

public class StringLit(string value) : Node
{
    readonly string Value = value;

    public override PwInstance Evaluate(ScopedSymbolTable scope) => new PwCsharpInstance(new PwString(Value));

    public override string ToPrettyString(int level) => AddSpaces(level, $"string: \"{Value}\"");
}

public class FloatLit(float value) : Node
{
    float Value = value;
    public override PwInstance Evaluate(ScopedSymbolTable scope) => new PwCsharpInstance(new PwNumeric(Value));

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, $"float: {value}");
        return s;
    }
}

public class BooleanLit(bool isTrue) : Node
{
    bool Value => isTrue;
    public override PwInstance Evaluate(ScopedSymbolTable scope) => new PwCsharpInstance(new PwBoolean(Value));

    public override string ToPrettyString(int level) => AddSpaces(level, $"bool: {(isTrue ? "true" : "false")}");
}