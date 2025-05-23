using Microsoft.Xna.Framework.Graphics;

namespace PlaywrightLang.LanguageServices.Object.Primitive;

internal static class PwObjectExtensions
{
    internal static bool IsNumeric(this object value)
    {
        return (value is (float or int or double or decimal));
    }

    /// <summary>
    /// Performs basic constant-time checks and attempts to return a Playwright instanced representation of the given object.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>PwInstance of object</returns>
    /// <exception cref="PwException"></exception>
    internal static PwInstance AsPwInstance(this object value)
    {
            if (value is PwCsharpInstance v)
                return v;
            
            if (value is null)
                return new PwNullInstance(); // null type
            
            if (value is bool b)
                return new PwCsharpInstance(new PwBoolean(b));
            
            if (IsNumeric(value))
                return new PwCsharpInstance(new PwNumeric((float)value));
             
            if (value is string s)
                return new PwCsharpInstance(new PwString(s));
            
            if (value is PwObjectClass o)
                return new PwCsharpInstance(o);
            
            throw new PwException("Object could not be converted to valid Playwright type. " +
                                  $"You might want class {value.GetType()} to inherit from PwObjectClass.");
    }
    /// <summary>
    /// Performs checks and returns a C# class representation of the Playwright object. 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="PwException"></exception>
    internal static PwObjectClass AsPwObjectClass(this object value)
    {
        if (value is bool b)
            return new PwBoolean(b);
        
        if (IsNumeric(value))
            return new PwNumeric((float)value);
         
        if (value is string s)
            return new PwString(s);

        if (value is PwObjectClass)
            return (PwObjectClass)value;
        
        throw new PwException("Object could not be converted to valid Playwright type. " +
                              "You might want this class to inherit from PwObjectClass.");
    }
    
}