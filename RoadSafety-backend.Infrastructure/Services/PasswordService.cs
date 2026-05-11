using System.Security.Cryptography;
using System.Text;
using RoadSafety_backend.Application.Interfaces;

namespace RoadSafety_backend.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 600000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    private const char Delimiter = ';';

    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithm,
            KeySize);

        return string.Join(Delimiter,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash),
            Iterations);
    }

    public bool Verify(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split(Delimiter);
        if (parts.Length != 3) return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] hash = Convert.FromBase64String(parts[1]);
        int iterations = int.Parse(parts[2]);

        byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithm,
            KeySize);

        return CryptographicOperations.FixedTimeEquals(hash, inputHash);
    }
}