namespace Mindflow_backend.iam.domain.model.commands;

public record VerifyPinCommand(int UserId, string Pin);