using System;
using System.Collections.Generic;
using System.Text;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.LanguageServices.AST;

internal interface IQualifiedIdentifier
{
    /// <summary>
    /// Set the value associated with this path or pure identifier.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="scope"></param>
    internal void Set(PwInstance obj, PwScope scope);
    
    internal PwInstance Evaluate(PwScope scope);
    
    internal string ToPrettyString(int level);

    internal string GetLastName();
}

public abstract class PwAst
{
    public abstract PwInstance Evaluate(PwScope scope);
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
internal class Block(string blockType, CompoundStmt stmt) : PwAst
{
    public override PwInstance Evaluate(PwScope scope)
    {
        return stmt.Evaluate(scope);
    }

    public override string ToPrettyString(int level)
    {
        string s = AddSpaces(level, $"{blockType} block:(\r\n");
        s += stmt.ToPrettyString(level + 1) + "\r\n";
        s += AddSpaces(level, ")");
        return s;
    }
}

internal class SceneBlock(string id, CompoundStmt stmt) : Block("scene", stmt)
{
    public override PwInstance Evaluate(PwScope scope)
    {
        //TODO: make scenes callable by the state.
        return stmt.Evaluate(scope);
    }
}

internal class Chunk : PwAst
{
    List<PwAst> _nodes = new();
    internal Chunk(params PwAst[] nodes)
    {
        _nodes.AddRange(nodes);
    }

    public override PwInstance Evaluate(PwScope scope)
    {
        try
        {
            foreach (PwAst _nd in _nodes)
            {
                _nd.Evaluate(scope);
            }
        }
        catch (PwExit)
        {

        }
        catch (PwReturn r)
        {
            return r.ReturnValue;
        } 

        PwState.Log("Evaluated chunk successfully");
        return null;
    }

    public override string ToPrettyString(int level)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("\r\nbegin chunk:\r\n");
        foreach (PwAst nd in _nodes)
        {
            sb.AppendLine(nd.ToPrettyString(level + 1)+",");
        }
        return sb.AppendLine("\r\nend chunk\r\n").ToString(); 
    }
}
