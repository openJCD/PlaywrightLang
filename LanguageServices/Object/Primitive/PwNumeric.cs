namespace PlaywrightLang.LanguageServices.Object.Primitive;

public class PwNumeric (float val) : PwObjectClass
{
    [PwItem("__value__")]
    float Value { get; set; } = val;

    public override object GetUnderlyingObject()
    {
        return Value;
    }

    [PwItem("__add__")]
    public override PwInstance PwAdd(PwInstance left, PwInstance right)
    {
        return ((float)left.GetUnderlyingObject() + (float)right.GetUnderlyingObject()).AsPwInstance();
    }
    
    
    [PwItem("__true__")]
    public override PwInstance PwTrue (PwInstance self)
    {
        if (Value > 0 || Value < 0)
        {
            return new PwCsharpInstance(new PwBoolean(true));
        }
        else
        {
            return new PwCsharpInstance(new PwBoolean(false));
        }
    }
}