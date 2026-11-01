using System.Security.Cryptography;
using System.Text;

namespace TindaKo.Services;

/// <summary>
/// PBKDF2 password hashing. Stored format: PBKDF2$iterations$saltBase64$hashBase64
/// The iteration count is stored inside the hash, so you can raise it later
/// without breaking existing users.
/// </summary>
public static class PasswordHasher
{
    private const string Prefix = "PBKDF2";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int DefaultIterations = 100_000;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password, salt, DefaultIterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Prefix}${DefaultIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    /// <summary>True if the stored value is already in the hashed format.</summary>
    public static bool IsHashed(string? stored) =>
        !string.IsNullOrEmpty(stored) && stored.StartsWith(Prefix + "$", StringComparison.Ordinal);

    /// <summary>
    /// Verifies a password. Also accepts legacy PLAINTEXT values so existing
    /// users can still log in once; rehash them right after (see IsHashed).
    /// </summary>
    public static bool Verify(string password, string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return false;

        if (!IsHashed(stored))
        {
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(password),
                Encoding.UTF8.GetBytes(stored));
        }

        var parts = stored.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations)) return false;

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
