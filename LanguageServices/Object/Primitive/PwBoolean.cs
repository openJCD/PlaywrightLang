namespace PlaywrightLang.LanguageServices.Object.Primitive;

public class PwBoolean(bool isTrue) : PwObjectClass
{
    [PwItem("__value__")] private bool Value = isTrue;
    public override object GetUnderlyingObject()
    {
        return Value;
    }
}