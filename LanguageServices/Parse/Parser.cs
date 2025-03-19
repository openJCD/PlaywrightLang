using System;
using System.Collections.Generic;
using System.Linq;
using PlaywrightLang.LanguageServices.AST;

namespace PlaywrightLang.LanguageServices.Parse;

public class Parser
{
    private List<Token> _tokenStream;
    private int _index;
    private Token CurrentToken => _tokenStream[_index];
    public Parser(List<Token> tokens)
    {
        _tokenStream = tokens;
    }
    
    #region Blocks
    
    public Chunk Parse()
    {
        List<Node> nodes = new();
        while (Peek().Type != TokenType.EOF)
        {
            nodes.Add(ParseBlock());
        }
        return new Chunk(nodes.ToArray());
    }

    public Node ParseBlock()
    {
        Node block = null;
        if (IsBlockToken(CurrentToken.Type))
        {
            
            //TODO: Parse various block types
            switch (CurrentToken.Type)
            {
                case TokenType.CastBlock:
                    block = ParseCastBlock();
                    Expect("Expected 'end'.", TokenType.EndBlock);
                    break;
                case TokenType.SceneBlock:
                    block = ParseSceneBlock();
                    break;
            }
        }
        else
        {
            return ParseStatement();
        }

        if (block!=null)
            return block;
        else
            throw Error(CurrentToken, "Expected block identifier ('scene', 'glossary', 'cast') or valid statement.");
    }

    private Node ParseSceneBlock()
    {
        Consume(); // consume 'scene'
        string id = Expect("Expected identifier for scene. ", TokenType.Name).Value;
        Expect("Expected ':'", TokenType.Colon);
        CompoundStmt cstmt = ParseCompoundStmt();
        return new SceneBlock(id, cstmt);
    }

    public Block ParseCastBlock()
    {
        Consume();
        Expect("Expected ':' after 'cast'.", TokenType.Colon);
        List<Statement> nodes = new();
        while (CurrentToken.Type != TokenType.EndBlock)
        {
            nodes.Add(ParseActorAssignment());
            Expect("Expected ';' after actor assignment.", TokenType.Semicolon);
        }
        CompoundStmt cstmt = new CompoundStmt(nodes.ToArray());
        return new Block("cast", cstmt);
    }
    
    public Statement ParseActorAssignment()
    {
        string left = Expect("Expected name as lvalue for actor assignment.", TokenType.Name).Value;
        Expect("Expected 'as' inside actor assignment.", TokenType.As);
        string type = Expect("Expected name as type for actor assignment.", TokenType.Name).Value;
        return new Statement(new ActorAssignment(left, type));
    }
    #endregion
    
    #region Statements

    public Statement ParseStatement()
    {
        Node expr = null;
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
            statements.Add(ParseStatement());
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
        Expect("Expected ':' after condition", TokenType.Colon);
        CompoundStmt stmt = ParseCompoundStmt();
        Token t = Expect("Expected 'end' or 'else'.", TokenType.EndBlock, TokenType.Else);
        //TODO: Implement if/else statements.
        return new IfStmt(condition, stmt);
    }
    
    public WhileLoop ParseWhileLoop()
    {
        Expect("Expected 'while'", TokenType.While);
        Expression condition = ParseExpression();
        Expect("Expected ':' after condition", TokenType.Colon);
        CompoundStmt stmt = ParseCompoundStmt();
        Expect("Expected 'end' to conclude loop condition.", TokenType.EndBlock);
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
            param = new DeclarationParameter(identifier, ParseAtom());
        }
        else
            param = new DeclarationParameter(identifier, null);

        return param;
    }
    
    #endregion
    
    #region Expressions
    public Expression ParseExpression()
    {
        //TODO: Implement expression parsing LOYAL TO THE GRAMMAR!!
        return new Expression(ParseAssignmentExpression());
    }

    public Node ParseAssignmentExpression()
    {
        Node lvalue = ParseLogicalOr();
        if (IsAssignmentToken(CurrentToken.Type))
        {
            if (lvalue.GetType() != typeof(Name) && lvalue.GetType() != typeof(DotOperator))
            {
                throw Error(CurrentToken, "Expected lvalue of assignment expression to be an identifier or qualified path.");
            }

            Token t = Consume();
            switch (t.Type)
            {
                case TokenType.Assignment:
                    return new AssignmentExpression((Name)lvalue, ParseAssignmentExpression());
                case TokenType.AddAssign:
                    return new IncrementalAssignment((Name)lvalue, ParseAssignmentExpression());
                case TokenType.SubAssign:
                    return new DecrementalAssignment((Name)lvalue, ParseAssignmentExpression());
                case TokenType.DivAssign:
                    return new DivAssignment((Name)lvalue, ParseAssignmentExpression());
                case TokenType.MultAssign:
                    return new DivAssignment((Name)lvalue, ParseAssignmentExpression());
            }
        }

        return lvalue;
    }

    public Node ParseLogicalOr()
    {
        Node lvalue = ParseLogicalAnd();
        while (CurrentToken.Type == TokenType.LogicalOr)
        {
            Expect("Expected '||' or 'or'", TokenType.LogicalOr);
            lvalue = new LogicalOr(lvalue, ParseLogicalAnd());
        }
        return lvalue;
    }

    public Node ParseLogicalAnd()
    {
        Node lvalue = ParseEqualityExpr();
        while (CurrentToken.Type == TokenType.LogicalAnd)
        {
            Expect("Expected 'and' or '&&'.", TokenType.LogicalAnd);
            lvalue = new LogicalAnd(lvalue, ParseEqualityExpr());
        }
        return lvalue;
    }

    public Node ParseEqualityExpr()
    {
        Node lvalue = ParseRelationalExpr();
        while (CurrentToken.Type == TokenType.Equal || CurrentToken.Type == TokenType.NotEqual)
        {
            Token t = Expect("Expected '==' or '!='", TokenType.Equal);
            if (CurrentToken.Type == TokenType.Equal)
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

    public Node ParseRelationalExpr()
    {
        Node lvalue = ParseAdditiveExpr();
        while (IsRelationalToken(CurrentToken.Type))
        {
            Token t = Expect("Expected relational operator.", TokenType.MoreThan, TokenType.LessThan, TokenType.MoreThanEq,
                TokenType.LessThanEq);
            switch (t.Type)
            {
                case TokenType.MoreThan:
                    lvalue = new GreaterThanOperator(lvalue, ParseAdditiveExpr());
                    break;
                case TokenType.LessThan:
                    lvalue = new LessThanOperator(lvalue, ParseAdditiveExpr());
                    break;
                case TokenType.MoreThanEq:
                    lvalue = new GreaterThanEqOperator(lvalue, ParseAdditiveExpr());
                    break;
                case TokenType.LessThanEq:
                    lvalue = new LessThanEqOperator(lvalue, ParseAdditiveExpr());
                    break;
            }
        }
        return lvalue;
    }

    public Node ParseAdditiveExpr()
    {
        Node lvalue = ParseMultiplicativeExpr();
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

    public Node ParseMultiplicativeExpr()
    {
        Node lvalue = ParsePathExpr();
        
        while (CurrentToken.Type is TokenType.Multiply or TokenType.Divide)
        {
            Token t = Expect("Expected '*' or '/'", TokenType.Multiply, TokenType.Divide);
            if (t.Type == TokenType.Multiply)
            {
                lvalue = new Multiply(lvalue, ParsePathExpr());
            }
            else
            {
                lvalue = new Divide(lvalue, ParsePathExpr());
            }
        }

        return lvalue;
    }

    public Node ParsePathExpr()
    {
        Node lvalue = ParsePrimary();
        while (CurrentToken.Type is TokenType.Dot or TokenType.Colon)
        {
            Token t = Consume();
            if (t.Type == TokenType.Dot)
            {
                lvalue = new DotOperator(lvalue, new Name(Expect("Expected name after '.'",TokenType.Name).Value));
            }
            else
            {
                lvalue = new ColonOperator(lvalue, (FunctionCall)ParseFunctionCall());
            }
        }

        return lvalue;
    }

    public Node ParsePrimary()
    {
        Node expr = null;
        if (IsUnaryPrefix(CurrentToken.Type))
        {
            Token t = Consume();
            switch (t.Type)
            {
                case TokenType.Not:
                    return new UnaryNot(ParsePrimary());
                case TokenType.Minus:
                    return new Negative(ParsePrimary());
            }
        } 
        else if (CurrentToken.Type == TokenType.LParen)
        {
            Consume();
            expr = ParseExpression();
            Expect("Expected ')' to close bracketed expression.", TokenType.RParen);
        }
        else 
        {
            expr = ParseAtom();
        }

        return expr;
    }

    public Node ParseAtom()
    {
        Token t = Expect("Expected atom: string literal, int literal, float literal, boolean literal or identifier.",
            TokenType.StringLiteral, 
            TokenType.IntLiteral,
            TokenType.FloatLiteral,
            TokenType.BoolFalse,
            TokenType.BoolTrue,
            TokenType.Name);
        switch (t.Type)
        {
            case (TokenType.StringLiteral) :
                return new StringLit(t.Value);
            case (TokenType.IntLiteral) :
                return new Integer(int.Parse(t.Value));
            case (TokenType.FloatLiteral):
                return new FloatLit(float.Parse(t.Value));
            case (TokenType.BoolTrue):
                return new BooleanLit(true);
            case (TokenType.BoolFalse):
                return new BooleanLit(false);
            case (TokenType.Name):
                return new Name(t.Value); 
        }

        throw Error(t , "Something went wrong - have you forgotten a semicolon?");
    }

    public Node ParseFunctionCall()
    {
        Token t = Expect("Expected identifier for function call.", TokenType.Name);
        Expect("Expected '(' to begin function parameters.", TokenType.LParen);
        ParamExpressions _params = (ParamExpressions)ParseParameterExprs();
        Expect("Expected ')' to conclude function parameters.", TokenType.RParen);
        return new FunctionCall(t.Value, _params);
    }

    public Node ParseParameterExprs()
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
            throw Error(t, message);
        }
    }
    
    public Token Consume()
    {
        if (_index + 1  < _tokenStream.Count)
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
        Console.Error.WriteLine($"Playwright parse error at {token.Line}:{token.Column}, {token.Type.ToString()}: \r\n ---> {message}");
        return new ParseException(message);
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

public class ParseException : Exception
{
    public ParseException(string message) : base(message) {}
}