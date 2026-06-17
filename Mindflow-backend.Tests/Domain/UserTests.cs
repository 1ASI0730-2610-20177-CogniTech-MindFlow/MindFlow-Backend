using Mindflow_backend.iam.domain.model.aggregates;

namespace Mindflow_backend.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Constructor_WithEmailAndPassword_HashesPassword()
    {
        var user = new User("test@mail.com", "Secret123");

        Assert.Equal("test@mail.com", user.Email);
        Assert.NotEqual("Secret123", user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("Secret123", user.PasswordHash));
    }

    [Fact]
    public void Constructor_WithGoogle_GeneratesRandomPasswordHash()
    {
        var user = new User("test@mail.com", "google-id-123", "John");

        Assert.Equal("test@mail.com", user.Email);
        Assert.Equal("John", user.Name);
        Assert.NotEmpty(user.PasswordHash);
    }

    [Fact]
    public void UpdatePasswordHash_ChangesHash()
    {
        var user = new User("test@mail.com", "OldPass");
        user.UpdatePasswordHash("NewPass");

        Assert.False(BCrypt.Net.BCrypt.Verify("OldPass", user.PasswordHash));
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPass", user.PasswordHash));
    }

    [Fact]
    public void UpdateProfile_SetsNameAndOccupation()
    {
        var user = new User("test@mail.com", "pass");
        user.UpdateProfile("Alice", "Developer");

        Assert.Equal("Alice", user.Name);
        Assert.Equal("Developer", user.Occupation);
    }

    [Fact]
    public void SetPin_And_VerifyPin_WorkCorrectly()
    {
        var user = new User("test@mail.com", "pass");
        Assert.False(user.HasPin);

        user.SetPin("1234");
        Assert.True(user.HasPin);
        Assert.True(user.VerifyPin("1234"));
        Assert.False(user.VerifyPin("0000"));
    }

    [Fact]
    public void RemovePin_ClearsPin()
    {
        var user = new User("test@mail.com", "pass");
        user.SetPin("5678");
        Assert.True(user.HasPin);

        user.RemovePin();
        Assert.False(user.HasPin);
        Assert.False(user.VerifyPin("5678"));
    }

    [Fact]
    public void LinkGoogle_SetsGoogleId()
    {
        var user = new User("test@mail.com", "pass");
        user.LinkGoogle("google-abc");

        Assert.Equal("google-abc", user.GoogleId);
    }
}
