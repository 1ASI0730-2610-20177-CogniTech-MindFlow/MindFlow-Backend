using System.Text.Json.Serialization;
using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.iam.domain.model.aggregates;

public partial class User : IAuditableEntity
{
    public int Id { get; private set; }
    public string Email { get; private set; }
    public string? Name { get; private set; }
    public string? Occupation { get; private set; }
    public string? GoogleId { get; private set; }

    [JsonIgnore]
    public string PasswordHash { get; private set; }

    [JsonIgnore]
    public string? PinHash { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public User(string email, string password)
    {
        Email = email;
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
    }

    public User(string email, string googleId, string? name)
    {
        Email = email;
        GoogleId = googleId;
        Name = name;
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());
    }

    public User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public void LinkGoogle(string googleId) => GoogleId = googleId;

    public User UpdatePasswordHash(string password)
    {
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        return this;
    }

    public User UpdateProfile(string? name, string? occupation)
    {
        Name = name;
        Occupation = occupation;
        return this;
    }

    public bool HasPin => PinHash is not null;

    public User SetPin(string pin)
    {
        PinHash = BCrypt.Net.BCrypt.HashPassword(pin);
        return this;
    }

    public bool VerifyPin(string pin) =>
        PinHash is not null && BCrypt.Net.BCrypt.Verify(pin, PinHash);

    public User RemovePin()
    {
        PinHash = null;
        return this;
    }
}