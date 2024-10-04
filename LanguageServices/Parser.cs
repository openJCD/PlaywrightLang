#nullable enable
using System.Collections.Generic;
using System.Linq;
namespace PlaywrightLang.LanguageServices;

abstract class NodeExpr
{
    public bool IsLeaf { get; private set; }
    private NodeExpr? l;
    public Token Token { get; protected set; }
    private NodeExpr? r;

    public NodeExpr GetLeft()
    {
        return l;
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

struct IntNodeExpr
{
    private Token t;
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

    AST BuildAbstractSyntaxTree()
    {
        // do precedence stuff : go through the list till you find a pattern matching 
        
        int index_lr = 0;
        AST tree = new AST();
        
        
        while (Peek().Type != TokenType.Null)
        {
            Token consumed_token = Consume();
            
        }

        return tree;
    }

    Token ComputeNumber()
    {
        while (Peek().Type != TokenType.Null)
        {
            Token consumed_token = Consume();
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
        
        return _tokens.ElementAt(_tokenIndex);
    }
}