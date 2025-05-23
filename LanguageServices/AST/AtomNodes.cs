using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Object.Primitive;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;

internal interface IAtomNode
{ 
    internal PwObjectClass AsPwObject(PwScope scope);
}

internal class Name(string identifier) : PwAst, IQualifiedIdentifier
{
    internal string Value { get; set; } = identifier;

    public override PwInstance Evaluate(PwScope scope)
    {
        //Parser.Log($"Found Name: {Value}");

        return scope.Lookup(Value);
    }

    public void Set(PwInstance obj, PwScope scope)
    {
        scope.MutateSymbol(Value, obj);
    }

    public override string ToPrettyString(int level) => AddSpaces(level, $"name: '{Value}'");
    public string GetLastName()
    {
        return Value;
    }
}
internal class Integer(int value) : PwAst, IAtomNode
{
    internal int Value { get; private set; } = value;

    public override PwInstance Evaluate(PwScope scope) => new PwCsharpInstance(new PwNumeric(Value));
    public PwObjectClass AsPwObject(PwScope scope)
    {
        return new PwNumeric(Value);
    }

    public override string ToPrettyString(int level)
    {
        return AddSpaces(level , $"int: {Value.ToString()}");
    }
}

internal class StringLit(string value) : PwAst
{
    readonly string Value = value;

    public override PwInstance Evaluate(PwScope scope) => new PwCsharpInstance(new PwString(Value));

    public override string ToPrettyString(int level) => AddSpaces(level, $"string: \"{Value}\"");
}

internal class FloatLit(float value) : PwAst
{
    float Value = value;
    public override PwInstance Evaluate(PwScope scope) => new PwCsharpInstance(new PwNumeric(Value));

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, $"float: {value}");
        return s;
    }
}

internal class BooleanLit(bool isTrue) : PwAst
{
    bool Value => isTrue;
    public override PwInstance Evaluate(PwScope scope) => new PwCsharpInstance(new PwBoolean(Value));

    public override string ToPrettyString(int level) => AddSpaces(level, $"bool: {(isTrue ? "true" : "false")}");
}