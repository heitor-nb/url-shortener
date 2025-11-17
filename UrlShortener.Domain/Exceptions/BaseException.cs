namespace UrlShortener.Domain.Exceptions;

public abstract class BaseException(string message) : Exception(message)
{
}
