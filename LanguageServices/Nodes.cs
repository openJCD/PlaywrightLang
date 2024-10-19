namespace PlaywrightLang.LanguageServices;
public abstract class Node
{
    public abstract object Evaluate();
    public abstract override string ToString();
}

public class Add : Node
{
    // replace this with expression node
    public readonly Node Left;
    
    public readonly Node Right;

    public Add(Node left, Node right)
    {
        this.Left = left;
        this.Right = right;
    }
    public override object Evaluate()
    {
        try
        {
            int l_val = int.Parse(Left.Evaluate().ToString());
            int r_val = int.Parse(Right.Evaluate().ToString());
            Parser.Log($"Addition: {ToString()}");
            return (l_val + r_val);
        }
        catch (Exception exception)
        {
            Parser.Log($"Unable to parse expression: {Left} + {Right}: {exception.Message}.");
            return null;
        }
    }

    public override string ToString()
    {
        return $"({Left} + {Right})";
    }
}
public class Subtract : Node
{
    public readonly Node Left;
    public readonly Node Right;
    public Subtract(Node left, Node right)
    {
        this.Left = left;
        this.Right = right;
    }
    
    public override object Evaluate()
    {
        try
        {
            int l_val = int.Parse(Left.Evaluate().ToString());
            int r_val = int.Parse(Right.Evaluate().ToString());
            Parser.Log($"Subtraction: {ToString()}");
            return (l_val - r_val);
        }
        catch
        {
            throw new Exception($"Unable to parse expression: {Left.Evaluate().ToString()} - {Right.Evaluate().ToString()}. Expected integer values.");
        }
    }
    public override string ToString()
    {
        return "(" + Left.ToString() + " - " + Right.ToString() + ")";
    }
}
public class Negative : Node
{
    public readonly Node Term;

    public Negative(Node term)
    {
        this.Term = term;
    }

    public override object Evaluate()
    {
        return -(int)Term.Evaluate();
    }

    public override string ToString()
    {
        return $"-({Term})";
    }
}
public class Multply : Node
{
    public readonly Node Left;
    public readonly Node Right;

    public Multply(Node left, Node right)
    {
        this.Left = left;
        this.Right = right;
    }

    public override object Evaluate()
    {
        Parser.Log($"Multiply: {this}");
        return (int)Left.Evaluate() * (int)Right.Evaluate();
    }

    public override string ToString()
    {
        return $"({Left} * {Right})";
    }
}
public class Divide : Node
{
    public readonly Node Left;
    public readonly Node Right;

    public Divide(Node left, Node right)
    {
        this.Left = left;
        this.Right = right;
    }
    
    public override object Evaluate()
    {
        Parser.Log($"Division: {this}");
        return (int)Left.Evaluate() / (int)Right.Evaluate();
    }

    public override string ToString()
    {
        return $"({Left} / {Right})";
    }
}
public class Name : Node
{
    public readonly string Identifier;

    public Name(string identifier)
    {
        this.Identifier = identifier;
    }
    // change this to access some kind of stack!
    public override object Evaluate()
    {
        Parser.Log($"Found Name: {Identifier}");
        try
        {
            PwObject obj = Parser.Objects[Identifier];
            if (obj.Type == PwObjectType.StringVariable)
                return obj.Data.ToString();
            if (obj.Type == PwObjectType.IntVariable)
                return (int)obj.Data;
        }
        catch
        {
            Parser.Log($"Not implemented: tried to evaluate name.");
        }
        return null;
    }
    public override string ToString() => Identifier;
}
public class Integer : Node {
    
    public int Value { get; private set; }

    public Integer(int value)
    {
        Value = value;
    }
    public override object Evaluate()
    {
        return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
