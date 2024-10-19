#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace PlaywrightLang.LanguageServices;

public class Parser
{
    private List<Token> _tokens;
    private int _tokenIndex = 0;
    private int _currentLine=1;
    private PlaywrightState _state;
    private Token _currentToken => Peek(0);
    private Token _lookahead => Peek(1);
    internal Dictionary<string, PwObject> Globals = new Dictionary<string, PwObject>();
    
    internal Dictionary<string, PwActor> Cast = new Dictionary<string, PwActor>();
    
    internal Dictionary<string, Type> ValidActorTypes = new Dictionary<string, Type>();
    
    internal Stack<PwObject> LocalStack { get; private set; } = new Stack<PwObject>();
    
    public Parser(List<Token> tokens, PlaywrightState state)
    {
        _tokens = tokens;
        _state = state;
    }

    public void ParseChunk()
    {
        while (Peek().Type != TokenType.Null)
        {
            ParseBlock();
        }
    }

    public void ParseBlock()
    {
        if (_currentToken.Type == TokenType.Newline)
        {
            Consume();
            _currentLine++;
        }
        while (_currentToken.Type is (TokenType.SceneBlock or TokenType.GlossaryBlock or TokenType.CastBlock))
        {
            switch (_currentToken.Type)
            {
                case TokenType.SceneBlock:
                    ParseSceneBlock();
                    break;
                case TokenType.GlossaryBlock:
                    ParseGlossaryBlock();
                    break;
                case TokenType.CastBlock:
                    ParseCastBlock();
                    break;
            }
            Consume();
        }
    }

    public void ParseSceneBlock()
    {
        Token name = Match(TokenType.Name);
        Match(TokenType.Colon);
        Match(TokenType.Newline);
        _currentLine++;
        while (_currentToken.Type is not TokenType.EndBlock)
        {
            ParseLine();
            Match(TokenType.Newline);
        }
        _currentLine++;
        // not really sure what to do here...
    }

    public void ParseGlossaryBlock()
    {
        Match(TokenType.GlossaryBlock);
        Match(TokenType.Colon);
        Match(TokenType.Newline);
        while (true)
        {
            if (_currentToken.Type is TokenType.EndBlock) return;
            ParseGlobalAssignment();
            Match(TokenType.Newline);
            _currentLine++;
        }
    }
    
    private void ParseGlobalAssignment()
    {
        if (_currentToken.Type == TokenType.Newline) return;
        string name = Match(TokenType.Name).Value;
        Match(TokenType.Assignment);
        object value = ParseExpression().Evaluate();
        if (value.GetType() == typeof(string))
        {
            Globals.Add(name, new PwObject(name, PwObjectType.StringVariable, value.ToString()));
        }
        else if (value.GetType() == typeof(int))
        {
            Globals.Add(name, new PwObject(name, PwObjectType.IntVariable, value));
        }
        Log($"Assigned value '{value}' to global '{name}'");
    }
    
    private void ParseCastBlock()
    {
        Match(TokenType.CastBlock);
        Match(TokenType.Colon);
        Match(TokenType.Newline);
        while (_currentToken.Type is not TokenType.EndBlock or TokenType.Null)
        {
            if (_currentToken.Type != (TokenType.Newline | TokenType.EndBlock))
                ParseActorAssignment();
            _currentLine++;
        }
        // not really sure what to do here...
    }


    public void ParseActorAssignment()
    {
        string name = Match(TokenType.Name).Value;
        Match(TokenType.As);
        string type = Match(TokenType.Name).Value;
    
        if (type == "actor")
            Cast.Add(name, new PwActor(name));
        else if (TypeExists(type))
        {
            Cast.Add(name, new PwActor(type));
        } 
    }
    
    public void ParseLine()
    {
        _currentLine++;
        while (_currentToken.Type is not TokenType.Newline)
        {
            Token id = Match(TokenType.Name);
            if (id.Value == "director")
            {
                // do director things 
                Match(TokenType.Colon);
                Log("Made director all");
            }
            else if (VariableExists(id.Value))
            {
                Match(TokenType.Colon);
                // change this later: functions called by actors will call those actors' own corresponding functions.
                ParseFunction();
            }
        }
    }
    
    public Node ParseFunction()
    {
        ThrowError("Functions have not yet been implemented.");
        return null;
    }

    public void DoSequence(string self, params string[] others)
    {
        ThrowError("Sequence has not yet been implemented.");
    }
    public Node ParseExpression()
    {
        Node l_val = ParseTerm();
        while (_currentToken.Type is TokenType.Plus or TokenType.Minus)
        {
            Token op = Consume();
            switch (op.Type)
            {
                case TokenType.Plus:
                    l_val = new Add(l_val, ParseTerm());
                    break;
                case TokenType.Minus:
                    l_val = new Subtract(l_val, ParseTerm());
                    break;
            }
        }
        return l_val; 
        
        // bail
        ThrowError("Could not find a valid expression to parse..?");
        return null;
    }
    
    public Node ParseTerm()
    {
        Node l_val = ParseFactor();
        while (_currentToken.Type == TokenType.Multiply || _currentToken.Type == TokenType.Divide)
        {
            Token op = Consume();
            switch (op.Type)
            {
                case TokenType.Multiply:
                    l_val = new Multiply(l_val, ParseFactor());
                    break;
                case TokenType.Divide:
                    l_val = new Divide(l_val, ParseFactor());
                    break;
                default: break;
            }
        }
        return l_val;
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
            case TokenType.StringLiteral:
                return new StringLit(t_current.Value);
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

    Token Match(TokenType t)
    {
        Token tk = Consume();
        if (tk.Type != t)
        {
            ThrowError($"Expected {t}, got {tk}");
            return Token.None;
        }
        return tk;
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

    public bool ActorExists(string actorName)
    {
        return Cast.ContainsKey(actorName);
    }

    public bool TypeExists(string type)
    {
        return ValidActorTypes.ContainsKey(type);
    }
    public T GetVariable<T>(string name)
    {
        Globals.TryGetValue(name, out PwObject value);
        return value.Get<T>();
    }
    internal void ThrowError(string err)
    {
        string msg = ($"At line '{_currentLine}' token '{_tokenIndex}': {err}");
        Log(msg, "ERROR");
    }

    public static void Log(string message, string tag = "INFO")
    {
        Console.Write($"• Playwright Parser ");
        if (tag == "ERROR")
            Console.ForegroundColor = ConsoleColor.Red;
        else if (tag == "WARNING")
            Console.ForegroundColor = ConsoleColor.Yellow;
        else if (tag == "INFO")
            Console.ForegroundColor = ConsoleColor.Blue;
        
        Console.Write($"[{tag}]: ");
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(message);
    }
}