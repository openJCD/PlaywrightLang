#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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
                    tokens.Add(new Token(TokenType.Newline, _currentLine, _currentColumn));
                    _currentLine++;
                    _currentColumn = 0;
                    break;
                case '+':
                    tokens.Add(new Token(TokenType.Plus, _currentLine, _currentColumn));
                    break;
                case '-':
                    tokens.Add(new Token(TokenType.Minus, _currentLine, _currentColumn));
                    break;
                case '*':
                    tokens.Add(new Token(TokenType.Multiply, _currentLine, _currentColumn));
                    break;
                case '/':
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
                    while (Peek() != '\n' && Peek() != '\0')
                    { Consume(); }
                    break;
                case ',':
                    tokens.Add(new Token(TokenType.Comma, _currentLine, _currentColumn));
                    break;
                default: break;
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
                    if (Peek() == '\\')
                    {
                        Consume();
                        _bufCurrent += Consume();
                    }
                }

                Consume();
                tokens.Add(new Token(TokenType.StringLiteral, _currentLine, _currentColumn, _bufCurrent));
                _bufCurrent = "";
            }
            
            if (!char.IsLetterOrDigit(ch_consumed))
                continue;
            
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
                    case "glossary":
                        tokens.Add(new Token(TokenType.GlossaryBlock, _currentLine, _currentColumn));
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
                    case "function":
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
                while (char.IsDigit(Peek()))
                {
                    _bufCurrent += Consume();
                }
                tokens.Add(new Token(TokenType.IntLiteral, _currentLine, _currentColumn, _bufCurrent));
                _bufCurrent = "";
            }
        }
        return tokens;
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
}

public enum TokenType
{
    Null, // used to signify the end of a list of tokens (EOF)
    Exit, // fin
    StringLiteral, // "<string of characters>"
    IntLiteral, // <any string of numbers with no decimal>
    Newline, // \n
    Plus, // +
    Minus, // -
    Multiply,// *
    Divide, // /
    Exponent, // ^
    Name, // <user-defined token name>
    SceneBlock, // scene
    GlossaryBlock, // glossary 
    CastBlock, // cast
    SequenceBlock, // sequence (equivalent to function)
    Colon, // :
    Assignment, // means
    LParen, // (
    RParen, // )
    Dot,// .
    EndBlock, // end
    As, // as -> (used in the cast block to denote a type of actor, for example 'tree as prop')
    Define,
    Func,
    For,
    While,
    Comma, 
    Return, // exeunt 
    With, // used in return statements: "exeunt with <expr>"
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
    public static Token None => new Token(TokenType.Null, 0, 0);
}