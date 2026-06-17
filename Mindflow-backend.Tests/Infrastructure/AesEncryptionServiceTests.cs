using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Encryption;

namespace Mindflow_backend.Tests.Infrastructure;

public class AesEncryptionServiceTests
{
    private static AesEncryptionService CreateService()
    {
        var key = AesEncryptionService.GenerateKey();
        return new AesEncryptionService(key);
    }

    [Fact]
    public void Encrypt_And_Decrypt_RoundTrips()
    {
        var service = CreateService();
        var plainText = "Este es un texto secreto del diario.";

        var cipher = service.Encrypt(plainText);
        var decrypted = service.Decrypt(cipher);

        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertextEachTime()
    {
        var service = CreateService();
        var plainText = "Mismo texto";

        var cipher1 = service.Encrypt(plainText);
        var cipher2 = service.Encrypt(plainText);

        Assert.NotEqual(cipher1, cipher2);
    }

    [Fact]
    public void Decrypt_WithDifferentKey_Throws()
    {
        var service1 = CreateService();
        var service2 = CreateService();

        var cipher = service1.Encrypt("Secret");

        Assert.ThrowsAny<Exception>(() => service2.Decrypt(cipher));
    }

    [Fact]
    public void Constructor_WithInvalidKeyLength_Throws()
    {
        var shortKey = Convert.ToBase64String(new byte[16]);

        Assert.Throws<ArgumentException>(() => new AesEncryptionService(shortKey));
    }

    [Fact]
    public void GenerateKey_Returns32BytesBase64()
    {
        var key = AesEncryptionService.GenerateKey();
        var bytes = Convert.FromBase64String(key);

        Assert.Equal(32, bytes.Length);
    }

    [Fact]
    public void Encrypt_And_Decrypt_HandlesUnicode()
    {
        var service = CreateService();
        var text = "日本語テスト 🎉 Ñoño café";

        var decrypted = service.Decrypt(service.Encrypt(text));

        Assert.Equal(text, decrypted);
    }

    [Fact]
    public void Encrypt_And_Decrypt_HandlesEmptyString()
    {
        var service = CreateService();

        var decrypted = service.Decrypt(service.Encrypt(""));

        Assert.Equal("", decrypted);
    }
}
