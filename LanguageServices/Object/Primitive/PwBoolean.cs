namespace PlaywrightLang.LanguageServices.Object.Primitive;

public class PwBoolean(bool isTrue) : PwObjectClass
{
    [PwItem("__value__")] private bool Value = isTrue;
    public override object GetUnderlyingObject()
    {
        return Value;
    }

    [PwItem("__true__")]
    public override bool PwTrue(PwInstance self)
    {
        return Value;
    }
}