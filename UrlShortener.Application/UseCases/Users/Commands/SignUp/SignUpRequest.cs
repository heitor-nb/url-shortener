using NetDevPack.SimpleMediator;
using UrlShortener.Application.Shared.Dtos;

namespace UrlShortener.Application.UseCases.Users.Commands.SignUp;

public record SignUpRequest(
    string Name,
    string Email,
    string Password
    ) : IRequest<UserDto>;
