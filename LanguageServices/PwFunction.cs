namespace PlaywrightLang.LanguageServices;

public class PwFunction : PwObject
{
    private Node Instructions;
    private PwActor caller;
    public readonly PwObjectType[] ArgumentTypes;
    
    public PwFunction(string id, Node instructions, PwActor actor, params PwObjectType[] argTypes) : base()
    {
        Name = id;
        ArgumentTypes = argTypes;
        Data = $"{Name}: {instructions}";
        Instructions = instructions;
        ObjType = PwObjectType.Function;
    }

    public Node Call(params object[] args)
    {
        return Instructions;
    }
}