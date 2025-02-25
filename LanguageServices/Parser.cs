#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
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
        Expect(TokenType.SceneBlock);
        Token name = Expect(TokenType.Name);
        Expect(TokenType.Colon);
        List<Node> children = new();
        _currentLine++;
        while (CurrentToken.Type is not TokenType.EndBlock)
        {
            Node line = ParseLine();
            if (line != null) children.Add(line);
        }

        Expect(TokenType.EndBlock);
        return new Block("scene", children.ToArray());
    }

    public Node ParseGlossaryBlock()
    {
        Expect(TokenType.GlossaryBlock);
        Expect(TokenType.Colon);
        List<Node> children = new List<Node>();
        while (CurrentToken.Type != TokenType.EndBlock)
        {
            Node line = ParseGlobalAssignment();
            if (line is not null) children.Add(line);
        }

        Expect(TokenType.EndBlock);
        return new Block("glossary", children.ToArray());
    }
    private Node ParseGlobalAssignment()
    {
        Node name = new StringLit(Expect(TokenType.Name).Value);
        Expect(TokenType.Assignment);
        Node right = ParseExpression();
        Expect(TokenType.Semicolon);
        return new GlobalAssigmnent(name, right, _state);
    }
    
    private Node ParseCastBlock()
    {
        Expect(TokenType.CastBlock);
        Expect(TokenType.Colon);
        List<Node> assignments = new();
        while (CurrentToken.Type is not (TokenType.EndBlock or TokenType.Null))
        {
            if (CurrentToken.Type != (TokenType.EndBlock))
            {
                Node asn = ParseActorAssignment();
                assignments.Add(asn);
            }
            _currentLine++;
        }

        Expect(TokenType.EndBlock);
        return new Block("cast", assignments.ToArray());
    }


    private Node ParseActorAssignment()
    {
        string name = Expect(TokenType.Name).Value;
        Expect(TokenType.As);
        string type = Expect(TokenType.Name).Value;
        Expect(TokenType.Semicolon);
        return new ActorAssignment(name, type, _state);
    }

    private Node ParseFuncBlock()
    {
        Expect(TokenType.Define);
        Expect(TokenType.Func);
        string func_id = Expect(TokenType.Name).Value;
        string for_actor = "all"; // if the 'for' is not specified, every actor has access to this function.
        if (CurrentToken.Type == TokenType.For)
        {
            Consume();   
            for_actor = Expect(TokenType.Name).Value;
        }
        Expect(TokenType.LParen);
        List<Name> args = new();
        while (CurrentToken.Type is not (TokenType.RParen))
        {
            string argname = Expect(TokenType.Name).Value;
            args.Add(new Name(argname, _state));
            if (CurrentToken.Type != TokenType.RParen)
                Expect(TokenType.Comma);
        }
        Expect(TokenType.RParen);
        Expect(TokenType.Colon);
        List<Node> instructions = new();
        while (CurrentToken.Type is not (TokenType.EndBlock or TokenType.Null))
        { 
            if (CurrentToken.Type == TokenType.Return)
            {
                Expect(TokenType.Return);
                if (CurrentToken.Type != TokenType.With)
                {
                    instructions.Add(new ReturnStmt(func_id, new VoidNode()));
                    Expect(TokenType.Semicolon);
                    continue;
                }
                Expect(TokenType.With);
                instructions.Add(new ReturnStmt(func_id, ParseExpression()));
                Expect(TokenType.Semicolon);
            } else if (CurrentToken.Type == TokenType.Name)
            {
                instructions.Add(ParseLine());
            }
        }
        
        Expect(TokenType.EndBlock);
        return new FunctionBlock(func_id, for_actor, args.ToArray(), instructions.ToArray(), _state);
    }
    
    private Node ParseLine()
    {
        string caller = Expect(TokenType.Name).Value;
        Expect(TokenType.Colon);
        List<Node> funccalls = new();
        funccalls.Add(ParseFunctionCall());
        Expect(TokenType.Semicolon);
        return new Line(CurrentToken.Line, caller, funccalls.ToArray());
    }

    private FunctionCall ParseFunctionCall()
    {
        string name = Expect(TokenType.Name).Value;
        Expect(TokenType.LParen);
        ParamExpressions args = ParseExprArgs();
        Expect(TokenType.RParen);
        return new FunctionCall(name, args, _state);
    }

    private ParamExpressions ParseExprArgs()
    {
        List<Node> args = new();
        while (true)
        {
            if (CurrentToken.Type == TokenType.RParen)
                break;
            Node expr = ParseExpression();
            args.Add(expr);
            if (CurrentToken.Type != TokenType.Comma)
                break;
            else 
                Expect(TokenType.Comma);
        }
        return new ParamExpressions(args.ToArray());
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
        Node primary_factor = null;
        switch (t_current.Type)
        {
            case TokenType.Minus:
                primary_factor = new Negative(ParseFactor());
                break;
            case TokenType.Name: 
                primary_factor = new Name(t_current.Value, _state);
                break;
            case TokenType.IntLiteral:
                primary_factor = new Integer(int.Parse(t_current.Value));
                break;
            case TokenType.LParen:
                Node inner = ParseExpression();
                Expect(TokenType.RParen);
                primary_factor = inner;
                break;
            case TokenType.StringLiteral:
                primary_factor = new StringLit(t_current.Value);
                break;
        }
        
        Node current_temporary_expr = primary_factor;
        List<Node> postfixes = new(); 
        postfixes.Add(current_temporary_expr);
        while (CurrentToken.Type == TokenType.Dot || CurrentToken.Type == TokenType.Colon)
        {
            switch (CurrentToken.Type)
            {
                case TokenType.Dot:
                    Expect(TokenType.Dot);
                    DotOperator op = new DotOperator(Expect(TokenType.Name).Value);
                    current_temporary_expr = op;
                    break;
                case TokenType.Colon:
                    Expect(TokenType.Colon);
                    FunctionCall func = ParseFunctionCall();
                    current_temporary_expr = func;
                    break;
            }
            postfixes.Add(current_temporary_expr);
        }

        if (postfixes.Count > 1)
        {
            return new Postfix(postfixes.ToArray());
        }
        else
        {
            return primary_factor;
        }
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

    Token Expect(TokenType t)
    {
        Token tk = Consume();
        if (tk.Type != t)
        {
            ThrowError($"Expected {t}, got {tk}");
            return Token.None;
        }
        return tk;
    }

    Token Expect(params TokenType[] tokens)
    {
        Token tk = Consume();
        if (!tokens.Contains(tk.Type))
        {
            ThrowError($"Expected any of {tokens}, got {tk}");
            return Token.None;
        }
        else
        {
            return tk;
        } 
    }

    bool IsBlock(TokenType t) => (t is (TokenType.CastBlock or TokenType.GlossaryBlock or TokenType.SceneBlock or TokenType.Define));
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