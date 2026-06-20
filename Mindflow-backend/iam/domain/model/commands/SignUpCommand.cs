namespace Mindflow_backend.iam.domain.model.commands;

public record SignUpCommand(string Email, string Password, string? Name = null);