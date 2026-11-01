using TindaKo.Models;

namespace TindaKo.Services;

/// <summary>
/// One-time conversion of old plaintext passwords to PBKDF2 hashes.
/// Safe to call on every startup: already-hashed users are skipped.
/// ONLY call it after your login code uses PasswordHasher.Verify,
/// otherwise nobody will be able to log in.
/// </summary>
public static class PasswordMigration
{
    public static async Task HashLegacyPasswordsAsync()
    {
        var users = await UserServices.GetUsersAsync();

        foreach (var user in users)
        {
            var stored = user.UserPassword;
            if (string.IsNullOrEmpty(stored) || PasswordHasher.IsHashed(stored))
                continue;

            user.UserPassword = await Task.Run(() => PasswordHasher.Hash(stored));
            await UserServices.UpdateUserAsync(user);
        }
    }
}
