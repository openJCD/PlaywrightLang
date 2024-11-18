#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security;

namespace PlaywrightLang.LanguageServices;

public class Parser
{
    private List<Token> _tokens;
    private int _tokenIndex = 0;
    private int _currentLine=1;
    private PwState _state;
    private Token CurrentToken => Peek(0);
    private Token Lookahead => Peek(1);
    public Parser(List<Token> tokens, PwState state)
    {
        _tokens = tokens;
        _state = state;
    }

    public Node ParseChunk()
    {
        List<Node> nodes = new();
        while (Peek().Type != TokenType.Null)
        {
            nodes.Add(ParseBlock());
            Match(TokenType.Newline);
        }

        return new Chunk(nodes.ToArray());
    }

    public Node ParseBlock()
    {
        switch (CurrentToken.Type)
        {
            case TokenType.SceneBlock:
                return ParseSceneBlock();
            case TokenType.GlossaryBlock:
                return ParseGlossaryBlock();
            case TokenType.CastBlock:
                return ParseCastBlock();
            case TokenType.Define:
                return ParseFuncBlock();
        }

        return new VoidNode();
    }

    public Node ParseSceneBlock()
    {
        Token name = Match(TokenType.Name);
        Match(TokenType.Colon);
        Match(TokenType.Newline);
        List<Node> children = new();
        _currentLine++;
        while (CurrentToken.Type is not TokenType.EndBlock)
        {
            Node line = ParseLine();
            if (line != null) children.Add(line);
            Match(TokenType.Newline);
        }
        _currentLine++;
        return new Block("scene", children.ToArray());
    }

    public Node ParseGlossaryBlock()
    {
        Match(TokenType.GlossaryBlock);
        Match(TokenType.Colon);
        Match(TokenType.Newline);
        List<Node> children = new List<Node>();
        while (true)
        {
            if (CurrentToken.Type is TokenType.EndBlock)
            {
                Match(TokenType.EndBlock);
                return new Block("glossary", children.ToArray());
            }
            Node line = ParseGlobalAssignment();
            if (line is not null) children.Add(line);
            Match(TokenType.Newline);
            _currentLine++;
        }

    }
    private Node ParseGlobalAssignment()
    {
        if (CurrentToken.Type == TokenType.Newline) return null;
        Node name = new StringLit(Match(TokenType.Name).Value);
        Match(TokenType.Assignment);
        Node right = ParseExpression();
        return new GlobalAssigmnent(name, right, _state);
    }
    
    private Node ParseCastBlock()
    {
        Match(TokenType.CastBlock);
        Match(TokenType.Colon);
        Match(TokenType.Newline);
        List<Node> assignments = new();
        while (CurrentToken.Type is not TokenType.EndBlock or TokenType.Null)
        {
            if (CurrentToken.Type != (TokenType.Newline | TokenType.EndBlock))
            {
                Node asn = ParseActorAssignment();
                assignments.Add(asn);
                Match(TokenType.Newline);
            }
            _currentLine++;
        }

        Match(TokenType.EndBlock);
        return new Block("cast", assignments.ToArray());
    }


    private Node ParseActorAssignment()
    {
        string name = Match(TokenType.Name).Value;
        Match(TokenType.As);
        string type = Match(TokenType.Name).Value;
        return new ActorAssignment(name, type, _state);
    }

    private Node ParseFuncBlock()
    {
        Match(TokenType.Define);
        Match(TokenType.Func);
        string func_id = Match(TokenType.Name).Value;
        string for_actor = "all"; // if the 'for' is not specified, every actor has access to this function.
        if (CurrentToken.Type == TokenType.For)
        {
            Consume();   
            for_actor = Match(TokenType.Name).Value;
        }
        Match(TokenType.LParen);
        List<Name> args = new();
        while (CurrentToken.Type is not (TokenType.RParen or TokenType.Newline))
        {
            string argname = Match(TokenType.Name).Value;
            args.Add(new Name(argname, _state));
            if (Peek().Type != TokenType.RParen)
                Match(TokenType.Comma);
        }
        Match(TokenType.RParen);
        Match(TokenType.Colon);
        List<Node> instructions = new();
        while (CurrentToken.Type is not (TokenType.EndBlock or TokenType.Null))
        {
            Token t = Consume();
            if (CurrentToken.Type == TokenType.Return)
            {
                Match(TokenType.Return);
                if (Peek().Type != TokenType.With)
                {
                    instructions.Add(new ReturnStmt(func_id, new VoidNode()));
                    continue;
                }
                Match(TokenType.With);
                instructions.Add(new ReturnStmt(func_id, ParseExpression()));
            } else if (CurrentToken.Type == TokenType.Name)
            {
                instructions.Add(ParseLine());
            }
            else Match(TokenType.Newline);
            
        }
        return new FunctionBlock(func_id, for_actor, args.ToArray(), instructions.ToArray(), _state);
    }
    
    private Node ParseLine()
    {
        _currentLine++;
        string caller = Match(TokenType.Name).Value;
        Match(TokenType.Colon);
        List<Node> funccalls = new();
        while (CurrentToken.Type is not TokenType.Newline)
        {
            funccalls.Add(ParseFunctionCall(caller));
            Match(TokenType.Dot);
        }

        return new Line(_currentLine, caller, funccalls.ToArray());
    }

    private Node ParseFunctionCall(string parent_actor)
    {
        ThrowError("Functions have not yet been implemented.");
        return null;
    }

    private Node ParseExpression()
    {
        Node l_val = ParseTerm();
        while (CurrentToken.Type is TokenType.Plus or TokenType.Minus)
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

    private Node ParseTerm()
    {
        Node l_val = ParseFactor();
        while (CurrentToken.Type == TokenType.Multiply || CurrentToken.Type == TokenType.Divide)
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

    private Node ParseFactor()
    {
        Token t_current = Consume();
        switch (t_current.Type)
        {
            case TokenType.Minus:
                return new Negative(ParseFactor());
            case TokenType.Name: 
                return new Name(t_current.Value, _state);
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

    bool IsBlock(TokenType t)
    {
        if (t is (TokenType.CastBlock or TokenType.GlossaryBlock or TokenType.SceneBlock or TokenType.Define))
        {
            return true;
        }

        return false;
    }
    internal void ThrowError(string err)
    {
        string msg = ($"At '{CurrentToken.Line}', '{CurrentToken.Column}',  token '{_tokenIndex}': {err}");
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