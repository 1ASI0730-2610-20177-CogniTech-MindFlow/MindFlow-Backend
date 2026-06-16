using System.Text.Json.Serialization;
using Mindflow_backend.Shared.Domain.Model.Entities;

namespace Mindflow_backend.iam.domain.model.aggregates;

public partial class User : IAuditableEntity
{
    public int Id { get; private set; }
    public string Email { get; private set; }
    
    [JsonIgnore]
    public string PasswordHash { get; private set; } 

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public User(string email, string passwordHash)
    {
        Email = email;
        PasswordHash = passwordHash;
    }

    public User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        return this;
    }
}