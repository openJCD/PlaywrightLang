using System;
namespace PlaywrightLang.LanguageServices;

public class PwException : Exception 
{
    public override string Message => $"Fatal Playwright Error: {error}";
    protected string error = "";
    public PwException() { }

    public PwException(string message) : base(message)
    {
        error = message;
    }
    public PwException(string message, Exception inner) : base(message, inner) { }
}

public class PwParseException : PwException
{
    public PwParseException(int ln, int col, string message) : base(message)
    {
        error = $"@{ln}:{col}: {message}";
    }
}


public class PwTypeException : PwException
{
    public PwTypeException(string t, string message) : base(message)
    {
        error = $"Error finding type {t}: {message}";
    }
}