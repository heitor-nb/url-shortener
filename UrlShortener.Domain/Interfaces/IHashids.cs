namespace UrlShortener.Domain.Interfaces;

public interface IHashids
{
    string Encode(int id);
    int Decode(string hash);
}
