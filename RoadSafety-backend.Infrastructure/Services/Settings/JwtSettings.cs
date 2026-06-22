namespace RoadSafety_backend.Infrastructure.Services.Settings;

public class JwtSettings
{
    public const int MinimumSecretBytes = 32;

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }

    public byte[] GetSecretBytes()
    {
        if (string.IsNullOrWhiteSpace(Secret))
            throw new InvalidOperationException("JwtSettings:Secret is not configured. Set JwtSettings__Secret.");

        var secretBytes = System.Text.Encoding.UTF8.GetBytes(Secret);
        if (secretBytes.Length < MinimumSecretBytes)
        {
            throw new InvalidOperationException(
                $"JwtSettings:Secret must be at least {MinimumSecretBytes} UTF-8 bytes for HS256. " +
                $"Current value is {secretBytes.Length} bytes.");
        }

        return secretBytes;
    }
}
