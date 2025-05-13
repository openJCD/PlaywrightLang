namespace PlaywrightLang.LanguageServices.Object.Primitive;

public class PwString(string val) : PwObjectClass
{

    [PwItem("__value__")]
    private string Value { get; set; } = val;

    [PwItem("parse_numeric")]
    public PwNumeric ParseNumeric()
    {
        return new PwNumeric(float.Parse(Value));
    }

    public override object GetUnderlyingObject()
    {
        return Value;
    }
}