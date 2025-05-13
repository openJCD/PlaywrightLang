using System;

namespace PlaywrightLang;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
public class PwItemAttribute : Attribute
{
    public string PwName { get; private set; }

    public PwItemAttribute(string name)
    {
        PwName = name;
    }
}