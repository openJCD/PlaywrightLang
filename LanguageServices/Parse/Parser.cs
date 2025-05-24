using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using PlaywrightLang.LanguageServices.AST;

namespace PlaywrightLang.LanguageServices.Parse;

internal class Parser
{
    private List<Token> _tokenStream;
    private int _index;
    public static string TextLog = "";
    
    private bool _hadError = false;
    private Token CurrentToken
    {
        get
        {
            if (_index < _tokenStream.Count)
                return _tokenStream[_index];
            else return Token.None;
        }
    }

    public Parser(List<Token> tokens)
    {
        _tokenStream = tokens;
    }
    
    #region Blocks
    
    public Chunk Parse()
    {
        List<PwAst> nodes = new();
        while (CurrentToken.Type != TokenType.EOF)
        {
            nodes.Add(ParseBlock());
        }

        if (_hadError) throw new ParseException("Could not continue: errors occurred while parsing.");
        
        return new Chunk(nodes.ToArray());
    }

    public PwAst ParseBlock()
    {
        PwAst block = null;
        try
        {
            if (IsBlockToken(CurrentToken.Type))
            {
                switch (CurrentToken.Type)
                {
                    case TokenType.SceneBlock:
                        block = ParseSceneBlock();
                        break;
                    // keeping as a switch in case of future block addition (which is unlikely)
                }
            }
            else
            {
                return ParseStatement();
            }
        }
        catch (ParseException e)
        {
            Token t = Consume();
            while (t.Type != TokenType.EndBlock && t.Type != TokenType.Semicolon)
            {
                if (t.Type == TokenType.EOF)
                {
                    break;
                }
                t = Consume();
            }
            return ParseBlock();
        }

        if (block!=null)
            return block;
        else
            throw Error(CurrentToken, "Expected block identifier ('scene', 'glossary', 'cast') or valid statement.");
    }

    private PwAst ParseSceneBlock()
    {
        Consume(); // consume 'scene'
        string id = Expect("Expected identifier for scene. ", TokenType.Name).Value;
        Expect("Expected ':'", TokenType.Colon);
        CompoundStmt cstmt = ParseCompoundStmt();
        return new SceneBlock(id, cstmt);
    }
    #endregion
    
    #region Statements

    public Statement ParseStatement() 
    {
        PwAst expr = null;

        switch (CurrentToken.Type)
        {
            case TokenType.Return:
                expr = ParseReturnStmt();
                Expect("Expected ';' after return statement.", TokenType.Semicolon);
                break;
            case TokenType.If:
                expr = ParseIfStmt();
                break;
            case TokenType.While:
                expr = ParseWhileLoop();
                break;
            case TokenType.Func:
                expr = ParseFunctionBlock();
                break;
            case TokenType.Enter:
                expr = ParseInstantiation();
                Expect("Expected ';' after instantiation.", TokenType.Semicolon);
                break;
            case TokenType.EOF:
                break;
            default:
                expr = ParseExpression();
                Expect("Expected ';' after expression statement.", TokenType.Semicolon);
                break;
        }
        
        return new Statement(expr);
    }

    public CompoundStmt ParseCompoundStmt()
    {
        
        List<Statement> statements = new();
        while (CurrentToken.Type != TokenType.EndBlock && CurrentToken.Type != TokenType.EOF)
        {
            try
            {
                statements.Add(ParseStatement());
            }
            catch (ParseException parseEx)
            {
                while (true)
                {
                    Token recovery_token = Consume();
                    if (recovery_token.Type == TokenType.Semicolon)
                    {
                        statements.Add(ParseStatement());
                        break;
                    }
                }
            }
        }

        return new CompoundStmt(statements.ToArray());
    }
    public ReturnStmt ParseReturnStmt()
    {
        Consume(); // consume the return token
        if (CurrentToken.Type == TokenType.With)
        {
            Consume();
            return new ReturnStmt(ParseExpression());
        }
        else
        {
            return new ReturnStmt(new VoidNode());
        }
    }

    public IfStmt ParseIfStmt()
    {
        Expect("Expected 'if'", TokenType.If);
        Expression condition = ParseExpression();
        Expect("Expected 'then' after if condition", TokenType.Then);
        CompoundStmt stmt = ParseCompoundStmt();
        Token t = Expect("Expected 'end' or 'else'.", TokenType.EndBlock, TokenType.Else);
        //TODO: Implement if/else statements.
        return new IfStmt(condition, stmt);
    }
    
    public WhileLoop ParseWhileLoop()
    {
        Expect("Expected 'while'", TokenType.While);
        Expression condition = ParseExpression();
        Expect("Expected 'do' after while condition", TokenType.Do);
        CompoundStmt stmt = ParseCompoundStmt();
        Expect("Expected 'end' to conclude loop.", TokenType.EndBlock);
        return new WhileLoop(condition, stmt);
    }

    public FunctionBlock ParseFunctionBlock()
    {
        Expect("Expected 'func'", TokenType.Func);
        Token name = Expect("Expected identifier", TokenType.Name);
        
        Token for_anyone = Expect("Expected 'for' followed by actor type identifier, or '(' ", TokenType.LParen, TokenType.For);
        string type = "all";
        if (for_anyone.Type == TokenType.For)
        {
            type = Expect("Expected actor type after 'for'.", TokenType.Name).Value;
            Expect("Expected '(' to begin parameters.", TokenType.LParen);
        }
        ParamNames parameters = ParseParamNames();
        Expect("Expected ')' after parameters", TokenType.RParen);
        Expect("Expected ':'", TokenType.Colon);
        CompoundStmt body = ParseCompoundStmt();
        Expect("Expected 'end'", TokenType.EndBlock);
        return new FunctionBlock(name.Value, type, parameters, body);
    }

    public ParamNames ParseParamNames()
    {
        ParamNames paramNames = new ParamNames(null, null);
        while (CurrentToken.Type == TokenType.Comma || CurrentToken.Type != TokenType.RParen)
        {
            paramNames = new ParamNames(paramNames, ParseDeclarationParameter());
            if (CurrentToken.Type == TokenType.RParen) break;
            Expect("Expected ',' to delimit declaration parameters.", TokenType.Comma);
        }

        return paramNames;
    }

    public DeclarationParameter ParseDeclarationParameter()
    {
        DeclarationParameter param = null;
        Name identifier =
            new Name(Expect("Expected name or single assignment for declaration parameter.", TokenType.Name).Value);
        if (CurrentToken.Type == TokenType.Assignment)
        {
            Consume();
            param = new DeclarationParameter(identifier, ParseAtom());
        }
        else
            param = new DeclarationParameter(identifier, null);

        return param;
    }

    public Instantiation ParseInstantiation()
    {
        Consume(); // consume 'enter' token
        string name = Expect("Expected instance name.", TokenType.Name).Value;
        Expect("Expected 'as' to separate instance name and type.", TokenType.As);
        Name typeName = new Name(Expect("Expected type name.", TokenType.Name).Value);
        Expect("Expected '(' to begin constructor arguments.", TokenType.LParen);
        ParamExpressions args = ParseParameterExprs() as ParamExpressions;
        Expect("Expected ')' to end constructor arguments.", TokenType.RParen);
        return new Instantiation(name, typeName, args);
    }
    #endregion
    
    #region Expressions
    public Expression ParseExpression()
    {
        return new Expression(ParseAssignmentExpression());
    }
    public PwAst ParseAssignmentExpression()
    {
        PwAst lvalue = ParseLogicalOr();
        if (IsAssignmentToken(CurrentToken.Type))
        {
            if (lvalue is not Name && lvalue is not AccessOperator)
            {
                throw Error(CurrentToken, "Expected lvalue of assignment expression to be an identifier or qualified path.");
            }

            var t = Consume();
            switch (t.Type)
            {
                case TokenType.Assignment:
                    return new AssignmentExpression((IQualifiedIdentifier)lvalue, ParseAssignmentExpression());
                case TokenType.AddAssign:
                    return new IncrementalAssignment((IQualifiedIdentifier)lvalue, ParseAssignmentExpression());
                case TokenType.SubAssign:
                    return new DecrementalAssignment((IQualifiedIdentifier)lvalue, ParseAssignmentExpression());
                case TokenType.DivAssign:
                    return new DivAssignment((IQualifiedIdentifier)lvalue, ParseAssignmentExpression());
                case TokenType.MultAssign:
                    return new MultAssignment((IQualifiedIdentifier)lvalue, ParseAssignmentExpression());
            }
        }

        return lvalue;
    }

    public PwAst ParseLogicalOr()
    {
        PwAst lvalue = ParseLogicalAnd();
        while (CurrentToken.Type == TokenType.LogicalOr)
        {
            Expect("Expected '||' or 'or'", TokenType.LogicalOr);
            lvalue = new LogicalOr(lvalue, ParseLogicalAnd());
        }
        return lvalue;
    }

    public PwAst ParseLogicalAnd()
    {
        PwAst lvalue = ParseEqualityExpr();
        while (CurrentToken.Type == TokenType.LogicalAnd)
        {
            Expect("Expected 'and' or '&&'.", TokenType.LogicalAnd);
            lvalue = new LogicalAnd(lvalue, ParseEqualityExpr());
        }
        return lvalue;
    }

    public PwAst ParseEqualityExpr()
    {
        PwAst lvalue = ParseRelationalExpr();
        while (CurrentToken.Type == TokenType.EqualTo || CurrentToken.Type == TokenType.NotEqual)
        {
            Token t = Consume();
            if (t.Type == TokenType.EqualTo)
            {
                lvalue = new EqualOperator(lvalue, ParseRelationalExpr());
            }
            else
            {
                lvalue = new NotEqualOperator(lvalue, ParseRelationalExpr());
            }
        }

        return lvalue;
    }

    public PwAst ParseRelationalExpr()
    {
        PwAst lvalue = ParseAdditiveExpr();
        while (IsRelationalToken(CurrentToken.Type))
        {
            Token t = Expect("Expected relational operator.", TokenType.MoreThan, TokenType.LessThan, TokenType.MoreThanEq,
                TokenType.LessThanEq);
            lvalue = t.Type switch
            {
                TokenType.MoreThan   => new GreaterThanOperator(lvalue, ParseAdditiveExpr()),
                TokenType.LessThan   => new LessThanOperator(lvalue, ParseAdditiveExpr()),
                TokenType.MoreThanEq => new GreaterThanEqOperator(lvalue, ParseAdditiveExpr()),
                TokenType.LessThanEq => new LessThanEqOperator(lvalue, ParseAdditiveExpr()),
                _ => lvalue
            };
        }
        return lvalue;
    }

    public PwAst ParseAdditiveExpr()
    {
        PwAst lvalue = ParseMultiplicativeExpr();
        while (CurrentToken.Type is TokenType.Plus or TokenType.Minus)
        {
            Token t = Expect("Expected '+' or '-'", TokenType.Plus, TokenType.Minus);
            if (t.Type == TokenType.Plus)
            {
                lvalue = new Add(lvalue, ParseMultiplicativeExpr());
            }
            else
            {
                lvalue = new Subtract(lvalue, ParseMultiplicativeExpr());
            }
        }

        return lvalue;
    }

    public PwAst ParseMultiplicativeExpr()
    {
        PwAst lvalue = ParseCallExpr();
        
        while (CurrentToken.Type is TokenType.Multiply or TokenType.Divide)
        {
            Token t = Expect("Expected '*' or '/'", TokenType.Multiply, TokenType.Divide);
            if (t.Type == TokenType.Multiply)
            {
                lvalue = new Multiply(lvalue, ParsePrimary());
            }
            else
            {
                lvalue = new Divide(lvalue, ParsePrimary());
            }
        }

        return lvalue;
    }
    public PwAst ParseCallExpr()
    {
        PwAst expr = ParsePrimary();
        while (true)
        {
            if (CurrentToken.Type == TokenType.Colon)
            {
                Expect("Expected ':' ", TokenType.Colon);
                Name name = new Name(Expect("Expected identifier", TokenType.Name).Value);
                expr = new AccessOperator(expr, name);
            }
            else if (CurrentToken.Type == TokenType.LParen)
            {
                Expect("Expected '(' ", TokenType.LParen);
                expr = new FunctionCall(expr, ParseParameterExprs());
                Expect("Expected ')'", TokenType.RParen);
            }
            else if (CurrentToken.Type == TokenType.LSqBracket)
            {
                Expect("Expected '['", TokenType.LSqBracket);
                expr = new IndexOperator(expr, ParseExpression());
                Expect("Expected ']'", TokenType.RSqBracket);
            }
            else break;
        }
        return expr;
    }

    public PwAst ParsePrimary()
    {
        PwAst expr = null;
        if (IsUnaryPrefix(CurrentToken.Type))
        {
            Token t = Consume();
            switch (t.Type)
            {
                case TokenType.Not:
                    return new UnaryNot(ParseCallExpr());
                case TokenType.Minus:
                    return new Negative(ParseCallExpr());
            }
        } 
        else if (CurrentToken.Type == TokenType.LParen)
        {
            Consume();
            expr = ParseExpression();
            Expect("Expected ')' to close bracketed expression.", TokenType.RParen);
        } else if (CurrentToken.Type == TokenType.LSqBracket)
        {
            Consume();
            List<PwAst> exprs = new List<PwAst>();
            while (true)
            {
                exprs.Add(ParseExpression());
                if (CurrentToken.Type != TokenType.RSqBracket)
                {
                    Expect("Expected ',' to delimit arguments", TokenType.Comma);
                }
                else break;
            }
            Expect("Expected ']' to close list literal.", TokenType.RSqBracket);
            return new ListLiteral(exprs.ToArray());
        }
        else 
        {
            expr = ParseAtom();
        }

        return expr;
    }

    
    
    public PwAst ParseAtom()
    {
        Token t = Expect("Expected atom: string literal, int literal, float literal, boolean literal or identifier.",
            TokenType.StringLiteral, 
            TokenType.IntLiteral,
            TokenType.FloatLiteral,
            TokenType.BoolFalse,
            TokenType.BoolTrue,
            TokenType.Name);
        return t.Type switch
        {
            TokenType.StringLiteral => new StringLit(t.Value),
            TokenType.IntLiteral    => new Integer(int.Parse(t.Value)),
            TokenType.FloatLiteral  => new FloatLit(float.Parse(t.Value)),
            TokenType.BoolTrue      => new BooleanLit(true),
            TokenType.BoolFalse     => new BooleanLit(false),
            TokenType.Name          => new Name(t.Value),
            _ => throw Error(t, "Something went wrong - have you forgotten a semicolon?")
        };
    }

    public ParamExpressions ParseParameterExprs()
    {
        ParamExpressions _params = new ParamExpressions(null, new VoidNode());
        
        while (CurrentToken.Type is TokenType.Comma || CurrentToken.Type != TokenType.RParen)
        {
            _params = new ParamExpressions(_params, ParseExpression());
            if (CurrentToken.Type == TokenType.RParen)
                break;
            Expect("Expected ',' to delimit parameters.", TokenType.Comma);
        }

        return _params;
    }
    
    #endregion
    
    public Token Expect(string message, params TokenType[] tokenTypes)
    {
        Token t = Consume();
        if (tokenTypes.Contains(t.Type))
        {
            return t;
        }
        else
        {
            throw Error(Peek(-1), message);
        }
    }
    
    public Token Consume()
    {
        if (_index < _tokenStream.Count)
        {
            return _tokenStream[_index++];
        }

        return Token.None;
    }

    public Token Peek(int lookAhead = 1)
    {
        if (_index + lookAhead < _tokenStream.Count)
        {
            return _tokenStream[_index + lookAhead];
        }

        return Token.None;
    }

    private bool IsBlockToken(TokenType t) => t == TokenType.SceneBlock || t == TokenType.CastBlock;

    private bool IsAssignmentToken(TokenType t) => t is TokenType.Assignment or TokenType.AddAssign or TokenType.SubAssign 
        or TokenType.MultAssign or TokenType.DivAssign;

    private bool IsRelationalToken(TokenType t) =>
        t is TokenType.MoreThan or TokenType.LessThan or TokenType.MoreThanEq or TokenType.LessThanEq;

    private bool IsUnaryPrefix(TokenType t) =>
        t is TokenType.Not or TokenType.Minus;
    
    private ParseException Error(Token token, string message)
    {
        string err = $"at {token.Line}:{token.Column}, {token.Type.ToString()}: \r\n ---> {message}\r\n";
        Log(err, "ERROR");
        _hadError = true;
        return new ParseException(message);
    }
    
    public static void Log(string message, string tag = "INFO")
    {
        string new_message = $"\r\n> Playwright Parser ";
        if (tag == "ERROR")
            Console.ForegroundColor = ConsoleColor.Red;
        else if (tag == "WARNING")
            Console.ForegroundColor = ConsoleColor.Yellow;
        else if (tag == "INFO")
            Console.ForegroundColor = ConsoleColor.Blue;
        
        new_message+=($"[{tag}]: {message}");
        
        Console.ForegroundColor = ConsoleColor.White;
        TextLog += new_message;
        Console.WriteLine(new_message);
    }

    public static string GetLog() => TextLog;
}

public class ParseException : Exception
{
    public ParseException(string message) : base(message) {}
}