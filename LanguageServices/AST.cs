using System;
using System.Collections.Generic;
using System.Linq;

namespace PlaywrightLang.LanguageServices;

public class AST
{
    public AstNode Root { get; private set; }

    public AST()
    {
    }

    void BuildTree()
    {

    }

}

public abstract class AstNode
{
    public AstNode? Left { get; set; }
    public AstNode? Right { get;  set; }
    public Token Token { get; set; }

    public AstNode(Token t, AstNode left, AstNode right)
    {
        Left = left;
        Token = t;
        Right = right;
    }

    public Token Evaluate()
    {
        if (Left == null || Right == null)
        {
            return Token;
        }

        // recursively traverse the tree
        Token l = Left.Evaluate();
        Token r = Right.Evaluate();
        switch (Token.Type)
        {
            case (TokenType.Multiply):
                if (l.Type == TokenType.IntLiteral && r.Type == TokenType.IntLiteral)
                {
                    int lv = Convert.ToInt32(l.Value);
                    int rv = Convert.ToInt32(r.Value);
                    return new Token(TokenType.IntLiteral, (lv * rv).ToString()); 
                }
                break;
            case (TokenType.Divide):
                if (l.Type == TokenType.IntLiteral && r.Type == TokenType.IntLiteral)
                {
                    int lv = Convert.ToInt32(l.Value);
                    int rv = Convert.ToInt32(r.Value);
                    return new Token(TokenType.Divide, (lv / rv).ToString());
                }

                break;
        }

        return Token;
    }
}
