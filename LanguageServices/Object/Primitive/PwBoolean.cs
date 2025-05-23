namespace PlaywrightLang.LanguageServices.Object.Primitive;

internal class PwBoolean(bool isTrue) : PwObjectClass
{
    [PwItem("__value__")] private bool Value = isTrue;
    public override object GetUnderlyingObject()
    {
        return Value;
    }

    [PwItem("__true__")]
    public override bool PwTrue()
    {
        return Value;
    }

    [PwItem("__not__")]
    public bool Not()
    {
        return !Value;
    }

    [PwItem("to_string")]
    public override string ToString()
    {
        return Value.ToString();
    }
}