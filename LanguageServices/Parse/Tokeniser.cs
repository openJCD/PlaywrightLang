#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Principal;
namespace PlaywrightLang.LanguageServices;

public class Tokeniser
{
    private string _filepath;
    private readonly char[] _codeChars;
    private string _codeRaw;
    private int _currentLine = 1;
    private int _currentColumn = 1;
    private int _readerIndex;
    public string Log = "";
    private char _currentCharacter => _codeChars[_readerIndex];
    public Tokeniser(string data)
    {
        _codeRaw = data;
        _codeChars = _codeRaw.ToCharArray();
    }

    public List<Token> Tokenise()
    {
        string _bufCurrent = "";
        List <Token> tokens = new List<Token>();
        while (Peek() != '\0')
        {
            char ch_consumed = Consume();
            switch (ch_consumed)
            {
                case '\n' :
                    _currentLine++;
                    _currentColumn = 0;
                    break;
                case '+':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.AddAssign, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        tokens.Add(new Token(TokenType.Plus, _currentLine, _currentColumn));
                    break;
                case '-':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.SubAssign, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        tokens.Add(new Token(TokenType.Minus, _currentLine, _currentColumn));
                    break;
                case '*':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.MultAssign, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        tokens.Add(new Token(TokenType.Multiply, _currentLine, _currentColumn));
                    break;
                case '/':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.DivAssign, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        tokens.Add(new Token(TokenType.Divide, _currentLine, _currentColumn));
                    break;
                case ':':
                    tokens.Add(new Token(TokenType.Colon, _currentLine, _currentColumn));
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LParen, _currentLine, _currentColumn));
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RParen, _currentLine, _currentColumn));
                    break;
                case '.':
                    tokens.Add(new Token(TokenType.Dot, _currentLine, _currentColumn));
                    break;
                case '#':
                    while (Peek() != '\n' && Peek() != '\0') { Consume(); }
                    break;
                case ',':
                    tokens.Add(new Token(TokenType.Comma, _currentLine, _currentColumn));
                    break;
                case ';':
                    tokens.Add(new Token(TokenType.Semicolon, _currentLine, _currentColumn));
                    break;
                case '=':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.EqualTo, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        tokens.Add(new Token(TokenType.Assignment, _currentLine, _currentColumn));
                    break;          
                case '<':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.LessThanEq, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        tokens.Add(new Token(TokenType.LessThan, _currentLine, _currentColumn));
                    break;
                case '>':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.MoreThanEq, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        tokens.Add(new Token(TokenType.MoreThan, _currentLine, _currentColumn));
                    break;
                case '!':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.NotEqual, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        tokens.Add(new Token(TokenType.Not, _currentLine, _currentColumn));
                    break;
                case '|':
                    if (Peek() == '|')
                    {
                        tokens.Add(new Token(TokenType.LogicalOr, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        throw Error("Incomplete 'logical or' token: '|' should be finished with another '|'.");
                    
                    break;
                case '&':
                    if (Peek() == '&')
                    {
                        tokens.Add(new Token(TokenType.LogicalAnd, _currentLine, _currentColumn));
                        Consume();
                    }
                    else
                        throw Error("Incomplete 'logical and' token: '&' should be finished with another '&'.");
                    break;
                case '[':
                    tokens.Add(new Token(TokenType.LSqBracket, _currentLine, _currentColumn));
                    break;
                case ']':
                    tokens.Add(new Token(TokenType.RSqBracket, _currentLine, _currentColumn));
                    break;
                default:
                    break;
            }
            // handle string literals
            if (ch_consumed == '"')
            {
                while (Peek() != '"' && Peek() != '\0')
                {
                    char _charCurrent = Consume();
                    _bufCurrent += _charCurrent;
                    if (Peek() == '\n')
                        Console.Error.WriteLine("Fatal error: Unterminated one-line string.");
                    // handle escape characters (does not work if they are not separated by a space.)
                    // if (Peek() == '\\')
                    // {
                    //     Consume();
                    //     _bufCurrent += Consume();
                    // }
                }

                Consume();
                tokens.Add(new Token(TokenType.StringLiteral, _currentLine, _currentColumn, _bufCurrent));
                _bufCurrent = "";
            }
            if (char.IsLetter(ch_consumed) || ch_consumed == '_')
            {
                _bufCurrent += ch_consumed;
                
                while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
                    _bufCurrent += Consume();

                switch (_bufCurrent.ToLower()) 
                {
                    case "fin":
                        tokens.Add(new Token(TokenType.Exit, _currentLine, _currentColumn));
                        break;
                    case "scene":
                        tokens.Add(new Token(TokenType.SceneBlock, _currentLine, _currentColumn));
                        break;
                    // equivalent to '=' in other languages
                    case "means": 
                        tokens.Add(new Token(TokenType.Assignment, _currentLine, _currentColumn));
                        break;
                    case "cast":
                        tokens.Add(new Token(TokenType.CastBlock, _currentLine, _currentColumn));
                        break;
                    case "sequence":
                        tokens.Add(new Token(TokenType.SequenceBlock, _currentLine, _currentColumn));
                        break;
                    case "end":
                        tokens.Add(new Token(TokenType.EndBlock, _currentLine, _currentColumn));
                        break;
                    case "as":
                        tokens.Add(new Token(TokenType.As, _currentLine, _currentColumn));
                        break;
                    case "define":
                        tokens.Add(new Token(TokenType.Define, _currentLine, _currentColumn));
                        break;
                    case "func":
                        tokens.Add(new Token(TokenType.Func, _currentLine, _currentColumn));
                        break;
                    case "for":
                        tokens.Add(new Token(TokenType.For, _currentLine, _currentColumn));
                        break;
                    case "while":
                        tokens.Add(new Token(TokenType.While, _currentLine, _currentColumn));
                        break;
                    case "exeunt":
                        tokens.Add(new Token(TokenType.Return, _currentLine, _currentColumn));
                        break;
                    case "with":
                        tokens.Add(new Token(TokenType.With, _currentLine, _currentColumn));
                        break;
                    case "true":
                        tokens.Add(new Token(TokenType.BoolTrue, _currentLine, _currentColumn));
                        break;
                    case "false":
                        tokens.Add(new Token(TokenType.BoolFalse, _currentLine, _currentColumn));
                        break;
                    case "more":
                        tokens.Add(FinishToken("more", "than", TokenType.MoreThan));
                        break;
                    case "less":
                        tokens.Add(FinishToken("less", "than", TokenType.LessThan));
                        break;
                    case "if":
                        tokens.Add(new Token(TokenType.If, _currentLine, _currentColumn));
                        break;
                    case "then":
                        tokens.Add(new Token(TokenType.Then, _currentLine, _currentColumn));
                        break;
                    case "do":
                        tokens.Add(new Token(TokenType.Do, _currentLine, _currentColumn));
                        break;
                    case "or":
                        tokens.Add(new Token(TokenType.LogicalOr, _currentLine, _currentColumn));
                        break;
                    case "and":
                        tokens.Add(new Token(TokenType.LogicalAnd, _currentLine, _currentColumn));
                        break;
                    case "else":
                        tokens.Add(new Token(TokenType.Else, _currentLine, _currentColumn));
                        break;
                    case "equals":
                        tokens.Add(new Token(TokenType.EqualTo, _currentLine, _currentColumn));
                        break;
                    case "enter":
                        tokens.Add(new Token(TokenType.Enter, _currentLine, _currentColumn));
                        break;
                    case "break":
                        tokens.Add(new Token(TokenType.Break, _currentLine, _currentColumn));
                        break;
                    case "continue":
                        tokens.Add(new Token(TokenType.Continue, _currentLine, _currentColumn));
                        break;
                    case "in":
                        tokens.Add(new Token(TokenType.In, _currentLine, _currentColumn));
                        break;
                    default:
                        tokens.Add(new Token(TokenType.Name, _currentLine, _currentColumn, _bufCurrent));
                        break;
                }
                _bufCurrent = "";
                
                continue;
            }
            // handle strings of digits for int literals
            if (char.IsDigit(ch_consumed))
            { 
                _bufCurrent = "";
                _bufCurrent += ch_consumed;
                while (char.IsDigit(Peek()) || Peek() == '.') 
                {
                    if (Peek() == '.')
                    {
                        if (char.IsNumber(Peek(1)))
                        {
                            _bufCurrent += Consume();
                        }
                        else
                            break;
                    }
                    else
                    {
                        _bufCurrent += Consume();
                    }
                }

                if (_bufCurrent.Contains(".") && _bufCurrent.Split(".").Length == 2)
                    tokens.Add(new Token(TokenType.FloatLiteral, _currentLine, _currentColumn, _bufCurrent));
                else
                    tokens.Add(new Token(TokenType.IntLiteral, _currentLine, _currentColumn, _bufCurrent));
                _bufCurrent = "";
            }
        }
        tokens.Add(new Token(TokenType.EOF, _currentLine, _currentColumn));
        return tokens;
    }

    /// <summary>
    /// Finishes the current two-word token, such as more than, less than...
    /// </summary>
    /// <param name="expected_word">The expected second word of the token</param>
    /// <returns>The finished token, or null if the word was invalid.</returns>
    public Token FinishToken(string first_word, string expected_word, TokenType final_type)
    {
        int initial_seek_position = _readerIndex;
        int initial_column = _currentColumn;
        while (!char.IsLetter(Peek()))
        {
            Consume(); //consume the current space or other character
        }
        string _wordBuffer = "";
        while (Peek() != ' ')
        {
            _wordBuffer += Consume();
        }
        while (Peek() == ' ')
        {
            Consume(); //consume the trailing spaces
        }
        if (_wordBuffer == expected_word)
        {
            return new Token(final_type, _currentLine, _currentColumn);
        }
        else
        {
            _currentColumn = initial_column;
            _readerIndex = initial_seek_position;
            throw Error($"Expected {expected_word} after {first_word} for {final_type.ToString()} at {_currentLine}:{_currentColumn}");
        }

        return Token.None;
    }
    char Peek(int ahead = 0)
    {
        if (_readerIndex + ahead >= _codeChars.Length)
            return char.MinValue;
        return _codeChars.ElementAtOrDefault(_readerIndex + ahead);
    }

    public char Consume()
    {
        char c = _codeChars.ElementAtOrDefault(_readerIndex++);
        _currentColumn++;
        return c;
    }

    public Exception Error(string err)
    {
        string msg = $"Playrwight Lexer [ERROR]: {err}";
        Log += msg + '\n';
        Console.WriteLine(msg);
        return new Exception(err);
    }
}

public enum TokenType
{
    EOF, // used to signify the end of a list of tokens (EOF)
    Exit, // fin
    StringLiteral, // "<string of characters>"
    IntLiteral, // \n
    Plus, // +
    Minus, // -
    Multiply,// *
    Divide, // /
    Exponent, // ^ TODO
    Name, // <user-defined token name>
    SceneBlock, // scene
    CastBlock, // cast
    SequenceBlock, // sequence (equivalent to function)
    Colon, // :
    Semicolon, // ; 
    Assignment, // means
    LParen, // (
    RParen, // )
    Dot,    // .
    EndBlock, // end
    As, // as -> (used in the cast block to denote a type of actor, for example 'tree as prop')
    Define,
    Func,
    For,
    While,
    Comma, 
    Return, // exeunt 
    With, // used in return statements: "exeunt with <expr>"
    Break, 
    Continue,
    BoolTrue,
    BoolFalse,
    Not, // 'not' | '!'
    MultAssign,
    DivAssign,
    AddAssign,
    SubAssign,
    EqualTo,    // ==
    NotEqual,   // != 
    MoreThan,   // > | more than
    LessThan,   // < | less than
    MoreThanEq, // >=
    LessThanEq, // <=
    If,
    LogicalOr,  // or | ||
    LogicalAnd, // and | &&
    FloatLiteral, // n.n ...
    Else,
    Then,
    Do,
    Enter, // enter
    LSqBracket, // [
    RSqBracket, // ]
    In
}

public struct Token
{
    public TokenType Type;
    public string? Value;
    public readonly int Line;
    public readonly int Column;

    public Token(TokenType type, int line, int column)
    {
        Line = line;
        Column = column;
        Type = type;
    }

    public Token(TokenType type, int line, int column, string? value) : this(type, line, column)
    {
        Value = value;
    }
    public override string ToString() => $"{Type}: {Value}";
    public static Token None => new Token(TokenType.EOF, 0, 0);
}