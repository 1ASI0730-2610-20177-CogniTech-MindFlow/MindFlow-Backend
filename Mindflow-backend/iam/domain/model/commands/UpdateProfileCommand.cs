namespace Mindflow_backend.iam.domain.model.commands;

public record UpdateProfileCommand(int UserId, string? Name, string? Occupation);
