#nullable enable
using System.Collections.Generic;
using System.Linq;
namespace PlaywrightLang.LanguageServices;

#region nodes
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
            return new Token(TokenType.IntLiteral, rv);
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
#endregion //nodes
public class Parser
{
    private readonly Dictionary<TokenType, int> BinaryOps =
        new Dictionary<TokenType, int>
        {
            { TokenType.Plus, 1 },
            { TokenType.Minus, 1 },
            { TokenType.Multiply, 2 },
            { TokenType.Divide, 2 }
        };
    public AST AbstractSyntaxTree { get; private set; }
    private List<Token> _tokens;
    private int _tokenIndex;
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }
    Token ComputeExpression(int initialPrecedence = 1)
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

    Token ComputeNumber()
    {
        // TODO: change this
        return new Token();
    }
    
    /// <summary>
    /// Increment the current token index and get token
    /// </summary>
    /// <returns>Current token before index is incremented</returns>
    Token Consume()
    {
        return _tokens.ElementAt(_tokenIndex++);
    }
    /// <summary>
    /// Look at the token with the given index offset.
    /// </summary>
    /// <param name="ahead">Index offset to look at (can be negative)</param>
    /// <returns>Peeked token, or Token.None if EOF encountered</returns>
    Token Peek(int ahead = 0)
    {
        if (_tokenIndex + ahead >= _tokens.Count || _tokenIndex + ahead < 0)
            return Token.None;
        
        return _tokens.ElementAt(_tokenIndex + ahead);
    }
}