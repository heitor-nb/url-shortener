using NSubstitute;
using NSubstitute.ReturnsExtensions;
using UrlShortener.Application.Exceptions;
using UrlShortener.Application.UseCases.Users.Commands.SignUp;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;
using UrlShortener.Tests.Helpers;

namespace UrlShortener.Tests.Application.Users;

public class SignUpHandlerHandle
{
    private readonly IUserRepos _userRepos = Substitute.For<IUserRepos>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();

    private SignUpHandler CreateHandler() => new(
        _userRepos,
        _passwordHasher,
        _unitOfWork
    );

    [Fact]
    public async Task ShouldThrowGivenEmailAlreadyExists()
    {
        var handler = CreateHandler();
        var request = new SignUpRequest("teste", "teste@email.com", "Senha123!");

        var existingUser = EntityFactory.CreateUser();

        _userRepos
            .GetByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        var ex = await Assert.ThrowsAsync<AppException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal("The informed email is already associated with an user.", ex.Message);
    }

    /*

    The case password is weak will be tested inside PasswordValidator tests.

    */


    [Fact]
    public async Task ShouldCreateUserGivenValid()
    {
        var handler = CreateHandler();
        var request = new SignUpRequest("teste", "teste@email.com", "Senha123!");

        _userRepos
            .GetByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .ReturnsNull();

        _passwordHasher.Hash(request.Password).Returns("hashed_password");

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Name, result.Name);

        await _userRepos
            .Received(1)
            .CreateAsync(
                Arg.Is<User>(u =>
                    u.Email.Address == request.Email &&
                    u.Name.Value == request.Name &&
                    u.Password == "hashed_password"
                ),
                Arg.Any<CancellationToken>()
            );

        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ShouldHashPasswordGivenValidData()
    {
        var handler = CreateHandler();
        var request = new SignUpRequest("teste", "teste@email.com", "Senha123!");

        _userRepos
            .GetByEmailAsync(request.Email, Arg.Any<CancellationToken>())
            .ReturnsNull();

        await handler.Handle(request, CancellationToken.None);

        _passwordHasher.Received(1).Hash(request.Password);
    }
}

