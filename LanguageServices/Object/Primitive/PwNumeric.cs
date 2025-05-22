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
    public float PwAdd(float left, float right)
    {
        return (left + right);
    }

    [PwItem("__sub__")]
    public float PwSubtract(float left, float right)
    {
        return (left - right);
    }

    [PwItem("__mul__")]
    public float PwMul(float left, float right)
    {
        return (left * right);
    }

    [PwItem("__div__")]
    public float PwDiv(float left, float right)
    {
        return (left / right);
    }
        
    [PwItem("to_string")]
    public string ToString()
    {
        return Value.ToString();
    }
    
    [PwItem("__true__")]
    public override bool PwTrue (PwInstance self)
    {
        if (Value > 0 || Value < 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}