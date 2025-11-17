using NetDevPack.SimpleMediator;
using UrlShortener.Application.Exceptions;
using UrlShortener.Application.Shared.Dtos;
using UrlShortener.Application.Shared.Mappings;
using UrlShortener.Application.Shared.Validators;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Interfaces;
using UrlShortener.Domain.Interfaces.Repositories;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Application.UseCases.Users.Commands.SignUp;

public class SignUpHandler : IRequestHandler<SignUpRequest, UserDto>
{
    private readonly IUserRepos _userRepos;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public SignUpHandler(
        IUserRepos userRepos,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork
    )
    {
        _userRepos = userRepos;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> Handle(
        SignUpRequest request,
        CancellationToken cancellationToken)
    {
        var email = new Email(request.Email);

        var existingUser = await _userRepos.GetByEmailAsync(email.Address, cancellationToken);

        if (existingUser != null) throw new AppException("The informed email is already associated with an user.");

        var name = new Name(request.Name);

        if (!PasswordValidator.Validate(request.Password)) throw new AppException("The informed password is weak.");

        var user = new User(
            name,
            email,
            _passwordHasher.Hash(request.Password)
        );

        await _userRepos.CreateAsync(user, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return user.ToDto();
    }
}
