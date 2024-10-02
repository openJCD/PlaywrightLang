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
    
    public Tokeniser(string filepath)
    {
        _filepath = filepath;
        Stream s = File.OpenRead(filepath);
        StreamReader sr = new StreamReader(s);
        _codeRaw = sr.ReadToEnd();
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
                default: break;
            }
            // handle string literals
            if (ch_consumed == '"')
            {
                while (Peek() != '"' && Peek() != '\0')
                {
                    _bufCurrent += Consume();
                    if (Peek() == '\n')
                        Console.Error.WriteLine("Fatal error: Unterminated one-line string.");
                }

                Consume();
                tokens.Add(new Token(TokenType.StringLiteral, _bufCurrent));
                _bufCurrent = "";
            }
            
            if (!char.IsLetterOrDigit(ch_consumed))
                continue;
            
            if (char.IsLetter(ch_consumed))
            {
                _bufCurrent += ch_consumed;
                
                while (char.IsLetterOrDigit(Peek()))
                    _bufCurrent += Consume();

                switch (_bufCurrent)
                {
                    case "fin":
                        tokens.Add(new Token(TokenType.Exit));
                        break;
                    case "actor":
                        tokens.Add(new Token(TokenType.Actor));
                        break;
                    // like a routine for the user to execute
                    case "scene":
                        tokens.Add(new Token(TokenType.Scene));
                        break;
                    // equivalent to '=' in other languages
                    case "means": 
                        tokens.Add(new Token(TokenType.Assignment));
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
    
    char Peek(int ahead = 1)
    {
        if (_readerIndex + ahead >= _codeChars.Length)
            return char.MinValue;
        return _codeChars.ElementAtOrDefault(_readerIndex);
    }

    public char Consume()
    {
        char c = _codeChars.ElementAtOrDefault(_readerIndex++);
        return c;
    }
}

public enum TokenType
{
    Exit,
    StringLiteral,
    IntLiteral,
    Newline,
    Plus,
    Minus, 
    Multiply,
    Divide,
    Name,
    Scene,
    Actor,
    Colon,
    Assignment
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
}