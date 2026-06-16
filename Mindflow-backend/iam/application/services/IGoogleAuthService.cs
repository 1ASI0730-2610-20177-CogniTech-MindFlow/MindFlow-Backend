namespace Mindflow_backend.iam.application.services;

public record GoogleUserInfo(string GoogleId, string Email, string? Name);

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> ValidateAsync(string credential);
}