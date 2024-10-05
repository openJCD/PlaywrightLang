#nullable enable
using System.Collections.Generic;
using System.Linq;
namespace PlaywrightLang.LanguageServices;

public abstract class NodeExpr
{
    public bool IsLeaf { get; private set; }
    private NodeExpr? l;
    public Token Token { get; protected set; }
    private NodeExpr? r;

    public NodeExpr GetLeft()
    {
        return l.GetLeft();
    }

    public Token Evaluate()
    {
        TokenType rt = r.Token.Type;
        TokenType lt = l.Token.Type;
        string rv = r.Token.Value;
        string lv = l.Token.Value;
        if (rt == TokenType.Multiply)
        {
            return new Token();
        }

        // replace this
        return Token.None;
    }
}

public class IntLeaf : NodeExpr
{
    public new bool IsLeaf => true;
    public int Value => int.Parse(Token.Value);
}

public class VariableLeaf : NodeExpr
{
    public new bool IsLeaf => true;
    public string Name => Token.Value;
} 
public class Parser
{
    public AST AbstractSyntaxTree { get; private set; }
    private List<Token> _tokens;
    private int _tokenIndex;
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }
    Token ComputeNumber()
    {

        while (Peek().Type != TokenType.Null)
        {
            NodeExpr left_current;
            Token consumed_token = Consume();
            NodeExpr right_current;
            
            if (consumed_token.Type == TokenType.IntLiteral)
            {
                if (Peek(-1).Type == TokenType.Null)
                {
                    left_current = new IntLeaf();
                }
            }
            
        }

        return Token.None;
    }
    Token Consume()
    {
        return _tokens.ElementAt(_tokenIndex++);
    }

    Token Peek(int ahead = 1)
    {
        if (_tokenIndex + ahead >= _tokens.Count || _tokenIndex + ahead < 0)
            return Token.None;
        
        return _tokens.ElementAt(_tokenIndex + ahead);
    }
}