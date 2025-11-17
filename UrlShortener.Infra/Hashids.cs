using UrlShortener.Domain.Interfaces;

namespace UrlShortener.Infra;

public class Hashids : IHashids
{   
    private readonly HashidsNet.Hashids _hashids = new();

    public int Decode(string hash) => _hashids.Decode(hash).Single();

    public string Encode(int id) => _hashids.Encode(id);
}
