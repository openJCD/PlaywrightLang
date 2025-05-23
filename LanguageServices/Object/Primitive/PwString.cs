namespace PlaywrightLang.LanguageServices.Object.Primitive;

internal class PwString(string val) : PwObjectClass
{

    [PwItem("__value__")]
    private string Value { get; set; } = val;

    [PwItem("parse_numeric")]
    public PwNumeric ParseNumeric()
    {
        return new PwNumeric(float.Parse(Value));
    }

    [PwItem("__add__")]
    public string PwAdd(string left, string right)
    {
        return left + right;
    }

    [PwItem("__sub__")]
    public string PwSub(string left, string right)
    {
        return left.Replace(right, "");
    }

    [PwItem("__eq__")]
    public bool PwEquals(string left, string right)
    {
        return left == right;
    }

    [PwItem("__neq__")]
    public bool PwNotEquals(string left, string right)
    {
        return left != right;
    }    
    public override object GetUnderlyingObject()
    {
        return Value;
    }
}