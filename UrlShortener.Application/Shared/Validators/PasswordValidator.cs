using System.Text.RegularExpressions;

namespace UrlShortener.Application.Shared.Validators;

public static partial class PasswordValidator
{
    public static bool Validate(
        string password
    )
    {
        var isValid = true;

        if (password.Length < 8
        || !password.Any(char.IsUpper)
        || !password.Any(char.IsLower)
        || !password.Any(char.IsNumber)
        || !ContainsSpecialChar().IsMatch(password)) isValid = false;

        return isValid;
    }

    [GeneratedRegex(@"[^a-zA-Z0-9\s]")]
    private static partial Regex ContainsSpecialChar();
}
