using UrlShortener.Domain.Exceptions;

namespace UrlShortener.Application.Exceptions;

public class AppException(string message) : BaseException(message)
{
}
