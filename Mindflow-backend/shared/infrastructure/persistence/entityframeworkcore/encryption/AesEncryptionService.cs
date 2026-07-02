using System.Security.Cryptography;
using System.Text;

namespace Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Encryption;

public sealed class AesEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(string base64Key)
    {
        _key = Convert.FromBase64String(base64Key);
        if (_key.Length != 32)
            throw new ArgumentException("Encryption key must be 256 bits (32 bytes) encoded as Base64.");
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[aes.BlockSize / 8];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    ///     Decrypts a value, falling back to the raw stored string when it cannot be
    ///     decrypted (legacy plaintext rows or data encrypted with a previous key).
    ///     Reading must never throw: a 500 on every journal/chat read is worse than
    ///     showing the stored value as-is.
    /// </summary>
    public string DecryptSafe(string cipherText)
    {
        try
        {
            return Decrypt(cipherText);
        }
        catch (Exception)
        {
            return cipherText;
        }
    }

    public static string GenerateKey()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        return Convert.ToBase64String(key);
    }
}
