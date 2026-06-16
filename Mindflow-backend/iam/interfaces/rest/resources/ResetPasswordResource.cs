namespace Mindflow_backend.iam.interfaces.rest.resources;

public record ResetPasswordResource(string Token, string NewPassword);
