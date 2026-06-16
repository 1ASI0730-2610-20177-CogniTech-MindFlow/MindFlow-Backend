namespace Mindflow_backend.iam.domain.model.commands;

public record ResetPasswordCommand(string Token, string NewPassword);
