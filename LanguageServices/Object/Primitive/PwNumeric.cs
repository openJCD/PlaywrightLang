namespace PlaywrightLang.LanguageServices.Object.Primitive;

public class PwNumeric (float val) : PwObjectClass
{
    [PwItem("__value__")]
    float Value { get; set; } = val;

    public override object GetUnderlyingObject()
    {
        return Value;
    }
}