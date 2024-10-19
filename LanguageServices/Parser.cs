#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;

namespace PlaywrightLang.LanguageServices;

public class Parser
{
    private List<Token> _tokens;
    private int _tokenIndex;
    private int _currentLine=1;
    private Token _currentToken => Peek(0);
    internal Dictionary<string, PwObject> Globals = new Dictionary<string, PwObject>();
    internal Stack <PwObject> LocalStack { get; private set; } = new Stack<PwObject>();
    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }
    
    public Node ParseExpression()
    {
        while (Peek().Type != (TokenType.Null))
        { 
            Node l_val = ParseTerm();
            while (_currentToken.Type is TokenType.Plus or TokenType.Minus)
            {
                Token t_op = Consume();
                switch (t_op.Type)
                {
                    // each statement here technically has the wrong order - first should be a ParseExpression, then a ParseTerm.
                    // this will hopefully be fixed later when ParseTerm and ParseFactor are implemented. 
                    case TokenType.Plus:
                        return new Add(l_val, ParseTerm());
                    case TokenType.Minus:
                        return new Subtract(l_val, ParseTerm());
                }

            }
            return l_val; 
        }
        // bail
        ThrowError("Could not find a valid expression to parse..?");
        return null;
    }
    
    public Node ParseTerm()
    {
        while (Peek().Type != TokenType.Null)
        {
            Node l_val = ParseFactor();
            while (_currentToken.Type == TokenType.Multiply || _currentToken.Type == TokenType.Divide)
            {
                Token op = Consume();
                switch (op.Type)
                {
                    case TokenType.Multiply:
                        return new Multiply(l_val, ParseFactor());
                    case TokenType.Divide:
                        return new Divide(l_val, ParseFactor());
                    default: break;
                }
            }
            return l_val;
        } 
        return null;
    }

    public Node ParseFactor()
    {
        Token t_current = Consume();
         switch (t_current.Type)
         {
             case TokenType.Minus:
                 return new Negative(ParseFactor());
             case TokenType.Name:
                 return new Name(t_current.Value, this);
             case TokenType.IntLiteral: 
                 return new Integer(int.Parse(t_current.Value));
             case TokenType.LParen:
                 Node inner = ParseExpression();
                 Token t_next = Consume();
                 if (t_next.Type != TokenType.RParen)
                 {
                     ThrowError($"Expected ')', got {t_next.Type}");
                     return null;
                 }
                 return inner;
         }
        
        return null;
    }
    
    /// <summary>
    /// Increment the current token index and get token
    /// </summary>
    /// <returns>Current token before index is incremented</returns>
    Token Consume()
    {
        if (Peek(0).Type != TokenType.Null)
            return _tokens.ElementAt(_tokenIndex++);
        return Token.None;
    }
    
    /// <summary>
    /// Look at the token with the given index offset.
    /// </summary>
    /// <param name="ahead">Index offset to look at (can be negative)</param>
    /// <returns>Peeked token, or Token.None if EOF encountered</returns>
    Token Peek(int ahead = 1)
    {
        if (_tokenIndex + ahead >= _tokens.Count || _tokenIndex + ahead < 0)
            return Token.None;
        return _tokens.ElementAt(_tokenIndex + ahead);
    }
    
    /// <summary>
    /// check if the 
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public bool VariableExists(string variableName)
    {
        return Globals.ContainsKey(variableName);
    }
    public T GetVariable<T>(string name)
    {
        Globals.TryGetValue(name, out PwObject value);
        return value.Get<T>();
    }
    internal void ThrowError(string err)
    {
        StringBuilder message = new StringBuilder();
        message.Append($"Parse error on line '{_currentLine}': {err}").AppendLine();
        Console.Error.Write(message.ToString());
    }

    public static void Log(string message)
    {
        Console.WriteLine($"• Playwright Parser: {message}");
    }
    bool IsBinop(TokenType op)
    {
        return op == TokenType.Plus || op == TokenType.Minus || op == TokenType.Multiply || op == TokenType.Divide;
    }
}