using Microsoft.Extensions.Configuration;
using UrlShortener.Domain.Interfaces;

namespace UrlShortener.Infra;

public class Hashids : IHashids
{
    private readonly HashidsNet.Hashids _hashids;

    public Hashids(
        IConfiguration cfg
    )
    {
        _hashids = new(
            salt: cfg["HashidsSalt"],
            minHashLength: 5
        );
    }

    public int Decode(string hash) => _hashids.Decode(hash).Single();

    public string Encode(int id) => _hashids.Encode(id);
}
