namespace PlaywrightLang.LanguageServices.Object.Primitive;

internal class PwNumeric (float val) : PwObjectClass
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

    [PwItem("__eq__")]
    public bool PwEquals(float left, float right)
    {
        return (left == right);
    }
    
    [PwItem("__true__")]
    public override bool PwTrue ()
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

    [PwItem("__not__")]
    public bool PwNot()
    {
        return !PwTrue();
    }    
    [PwItem("__neq__")]
    public bool PwNotEquals(float left, float right)
    {
        return left != right;
    }

    [PwItem("__lt__")]
    public bool PwLt(float left, float right)
    {
        return (left < right);
    }

    [PwItem("__gt__")]
    public bool PwGt(float left, float right)
    {
        return (left > right);
    }
    
    [PwItem("__geq__")]
    public bool PwGeq(float left, float right)
    {
        return (left >= right);
    }
    [PwItem("__leq__")]
    public bool PwLeq(float left, float right)
    {
        return (left <= right);
    }
    
    [PwItem("to_string")]
    public string ToString()
    {
        return Value.ToString();
    }
}