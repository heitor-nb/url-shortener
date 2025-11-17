using UrlShortener.Application.Shared.Dtos;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Shared.Mappings;

public static class UserMappings
{
    public static UserDto ToDto(this User user) => new(
        user.Name.Value,
        user.Email.Address
    );
}
