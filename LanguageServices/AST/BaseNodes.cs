using System;
using System.Collections.Generic;
using System.Text;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;

public interface IQualifiedIdentifier
{
    /// <summary>
    /// Set the value associated with this path or pure identifier.
    /// </summary>
    /// <param name="value"></param>
    public void Set(object value);
    public string ToPrettyString(int level);
}

public abstract class Node
{
    public abstract PwInstance Evaluate(ScopedSymbolTable scope);
    protected int Level = 0;
    
    public abstract string ToPrettyString(int level);

    protected string AddSpaces(int level, string input)
    {
        string o = '|' + input;
        for (int i = 0; i < level; i++)
        {
            o = o.Insert(1, "-");
            if (i == level - 1)
            {
                o = o.Insert(level+1, " ");
            }
        }

        return o;
    }
}
public class Block(string blockType, CompoundStmt stmt) : Node
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        stmt.Evaluate(scope);
        return null;
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, $"{blockType} block:(\r\n");
        s += stmt.ToPrettyString(level + 1) + "\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

public class SceneBlock(string id, CompoundStmt stmt) : Block("scene", stmt)
{
    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        throw new NotImplementedException();
    }
}

public class Chunk : Node
{
    List<Node> _nodes = new();
    public Chunk(params Node[] nodes)
    {
        _nodes.AddRange(nodes);
    }

    public override PwInstance Evaluate(ScopedSymbolTable scope)
    {
        foreach (Node _nd in _nodes)
        {
            _nd.Evaluate(scope); 
        }

        Parser.Log("parsed chunk successfully");
        return null;
    }

    public override string ToPrettyString(int level)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("\r\nbegin chunk:\r\n");
        foreach (Node nd in _nodes)
        {
            sb.AppendLine(nd.ToPrettyString(level + 1)+",");
        }
        return sb.AppendLine("\r\nend chunk\r\n").ToString(); 
    }
}
