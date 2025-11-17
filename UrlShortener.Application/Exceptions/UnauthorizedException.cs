namespace UrlShortener.Application.Exceptions;

public class UnauthorizedException(string message) : Exception(message);
