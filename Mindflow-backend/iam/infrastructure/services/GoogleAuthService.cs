using Google.Apis.Auth;
using Mindflow_backend.iam.application.services;

namespace Mindflow_backend.iam.infrastructure.services;

public class GoogleAuthService(IConfiguration configuration) : IGoogleAuthService
{
    public async Task<GoogleUserInfo?> ValidateAsync(string credential)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [configuration["Google:ClientId"]!]
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

            return new GoogleUserInfo(payload.Subject, payload.Email, payload.Name);
        }
        catch
        {
            return null;
        }
    }
}