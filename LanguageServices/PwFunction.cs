namespace PlaywrightLang.LanguageServices;

public class PwFunction : PwObject
{
    private Node Instructions;
    public readonly PwObjectType[] ArgumentTypes;
    public PwFunction(string id, Node instructions, params PwObjectType[] args) : base()
    {
        Name = id;
        ArgumentTypes = args;
        Data = Name + instructions;
        Instructions = instructions;
        Type = PwObjectType.Sequence;
    }

    public Node Call(params object[] args)
    {
        return Instructions;
    }
}