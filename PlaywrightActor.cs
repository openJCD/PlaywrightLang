namespace PlaywrightLang;

/// <summary>
/// Represents an Actor object in a Playwright script - allows for fully custom implementation.
/// Contains basic built-in methods - (for Playwright developer) Please try to keep members to a minimum for
/// ease of use. 
/// </summary>
public interface PlaywrightActor
{
    string Name { get; set; }
    int XPos { get; set; }
    int YPos { get; set; }
    void Say(string dialogue);
    void Ready();
    void Destroy();
}