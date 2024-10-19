namespace PlaywrightLang;

/// <summary>
/// Represents an Actor object in a Playwright script - allows for fully custom implementation.
/// Contains basic built-in methods - (for Playwright developer) Please try to keep members to a minimum for
/// ease of use. 
/// </summary>
public class PwActor
{
    string Name { get; set; }
    int XPos { get; set; }
    int YPos { get; set; }

    public PwActor(string name)
    {
        Name = name;
    }
    public virtual void Say(string dialogue) {}
    public virtual void Ready() {}
    public virtual void Destroy() {}
}