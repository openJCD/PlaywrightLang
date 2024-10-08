#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;

namespace PlaywrightLang.LanguageServices;

#region nodes
public abstract class NodeExpr
{
    public bool IsLeaf { get; private set; }
    private NodeExpr? l;
    public Token Token { get; protected set; }
    private NodeExpr? r;

    public Dictionary<string, PwObject> Objects { get; private set; } = new Dictionary<string, PwObject>();
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
    private List<Token> _tokens;
    private int _tokenIndex;
    private int _currentLine=1;
    
    public Dictionary<string, PwObject> Objects = new Dictionary<string, PwObject>();
    
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public void DoParse()
    {
        while (Peek().Type != TokenType.Null)
        { 
            int number = int.Parse(ComputeExpression().Value?.ToString());
            Console.WriteLine("Calculated int: " + number);
            Token T = Consume();
            if (T.Type == TokenType.Newline)
            {
                _currentLine++;
            }
            if (T.Type == TokenType.Name)
            {
                while (Peek().Type != TokenType.Newline)
                { 
                    Token T2 = Consume();
                    if (T2.Type != TokenType.Assignment) continue;
                    Token val = Consume();
                    
                    if (val.Type == TokenType.IntLiteral)
                        Objects[T.Value] = new PwObject(T.Value, PwObjectType.IntVariable, val.Value);
                    else if (val.Type == TokenType.StringLiteral)
                        Objects[T.Value] = new PwObject(T.Value, PwObjectType.StringVariable, val.Value);
                }
            }

        }
    }
    Token ComputeExpression(int initialPrecedence = 1)
    {
        
        Token current = Consume();
        Token resultLeft = ComputeNumber();
        Console.WriteLine("Calculated int: " + resultLeft.Value);
        while (Peek().Type != TokenType.Null)
        {
            current = Consume();
            // taken from pseudocode: while current token is a binary operator with precedence >= minimum precedence
            if (GetPrec(current.Type) >= initialPrecedence)
            {
                int precedence = GetPrec(current.Type);
                // Associativity assoc = GetAssoc(current.Type)
                int nextPrecedence = precedence + 1;
                Token resultRight = ComputeExpression(nextPrecedence); // recursively travel down the right of the tree 
                Token resultOverall = Calculate(current.Type, resultLeft, resultRight);
                return resultOverall;
            } 
        } 
        return ComputeNumber();
    } 
    Token ComputeNumber()
    {
        // taken from eli.thegreenplace.net, re-written in c# and for this 
        // parser structure.

        // recursively handle expressions in brackets.
        if (Peek().Type == TokenType.LParen)
        {
            Consume();
            // actually calculate expression inside brackets (recursively)
            Token value = ComputeExpression(1);
            
            // throw an error in the event of 
            if (Peek().Type != TokenType.RParen)
                Console.Error.WriteLine($"Expected ')' but got '{Peek().Type}'");
            Consume();
            return value;
        }

        if (Peek().Type == TokenType.Null)
            Console.Error.WriteLine($"Got Null token (file ended unexpectedly)");
        else if (BinaryOps.Keys.Contains(Peek().Type)) // if the peeked token is actually a math operator...
            ThrowError($"Got a binary operator {Peek().Type} when expecting int literal or bracketed expression.");
        /* else if (Peek().Type == TokenType.Name && Peek(-1).Type == TokenType.Assignment) // handle variables
        {
            string name = Consume().Value;
            return new Token(TokenType.IntLiteral, GetVariable<int>(name).ToString());
        } */
        else if (Peek().Type == TokenType.IntLiteral)
            return Consume();
        
        // return a null token if all else fails
        ThrowError($"Failed to compute expression (unexpected error). Bailing out.");
        return Token.None;
    }

    int GetPrec(TokenType t)
    {
        if (BinaryOps.Keys.Contains(t))
            return BinaryOps[t];
        else return 0;
    }
    Token Calculate(TokenType operand, Token left, Token right)
    {
        int lv = int.Parse(left.Value);
        int rv = int.Parse(right.Value);
        switch (operand)
        {
            case TokenType.Plus:
                return new Token(TokenType.IntLiteral, (lv + rv).ToString());
                break;
            case TokenType.Minus: 
                return new Token(TokenType.IntLiteral, (lv - rv).ToString());
                break;
            case TokenType.Multiply:
                return new Token(TokenType.IntLiteral, (lv * rv).ToString());
                break;
            case TokenType.Divide:
                return new Token(TokenType.IntLiteral, (lv / rv).ToString());
                break;
            default:
                ThrowError("Found unexpected operand.");
                return Token.None;
        }
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
    public T GetVariable<T>(string name)
    {
        Objects.TryGetValue(name, out PwObject value);
        return value.Get<T>();
    }
    void ThrowError(string err)
    {
        StringBuilder message = new StringBuilder();
        message.Append($"Parse error on line '{_currentLine}': {err}").AppendLine();
        Console.Error.Write(message.ToString());
    }
}