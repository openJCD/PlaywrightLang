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
    private string _currentLine;
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
                    tokens.Add(new Token(TokenType.Newline));
                    break;
                case '+':
                    tokens.Add(new Token(TokenType.Plus));
                    break;
                case '-':
                    tokens.Add(new Token(TokenType.Minus));
                    break;
                case '*':
                    tokens.Add(new Token(TokenType.Multiply));
                    break;
                case '/':
                    tokens.Add(new Token(TokenType.Divide));
                    break;
                case ':':
                    tokens.Add(new Token(TokenType.Colon));
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LParen));
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RParen));
                    break;
                case '.':
                    tokens.Add(new Token(TokenType.Dot));
                    break;
                case '#':
                    while (Peek() != '\n' && Peek() != '\0')
                    { Consume(); }
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
                tokens.Add(new Token(TokenType.StringLiteral, _bufCurrent));
                _bufCurrent = "";
            }
            
            if (!char.IsLetterOrDigit(ch_consumed))
                continue;
            
            if (char.IsLetter(ch_consumed) || ch_consumed == '_')
            {
                _bufCurrent += ch_consumed;
                
                while (char.IsLetterOrDigit(Peek()) || Peek() == '_')
                    _bufCurrent += Consume();

                switch (_bufCurrent)
                {
                    case "fin":
                        tokens.Add(new Token(TokenType.Exit));
                        break;
                    // like a routine for the user to execute
                    case "scene":
                        tokens.Add(new Token(TokenType.SceneBlock));
                        break;
                    // equivalent to '=' in other languages
                    case "means": 
                        tokens.Add(new Token(TokenType.Assignment));
                        break;
                    case "glossary":
                        tokens.Add(new Token(TokenType.GlossaryBlock));
                        break;
                    case "cast":
                        tokens.Add(new Token(TokenType.CastBlock));
                        break;
                    case "sequence":
                        tokens.Add(new Token(TokenType.SequenceBlock));
                        break;
                    case "end":
                        tokens.Add(new Token(TokenType.EndBlock));
                        break;
                    case "as":
                        tokens.Add(new Token(TokenType.As));
                        break;
                    default:
                        tokens.Add(new Token(TokenType.Name, _bufCurrent));
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
                tokens.Add(new Token(TokenType.IntLiteral, _bufCurrent));
                _bufCurrent = "";
                continue;
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
    As // as -> (used in the cast block to denote a type of actor, for example 'tree as prop')
}

public struct Token
{
    public TokenType Type;
    public string? Value;
    
    public Token(TokenType type)
    {
        Type = type;
    }
    public Token(TokenType type, string? value)
    {
        Type = type;
        Value = value;
    }
    public override string ToString() => $"{Type}: {Value}";
    public static Token None => new Token(TokenType.Null);
}