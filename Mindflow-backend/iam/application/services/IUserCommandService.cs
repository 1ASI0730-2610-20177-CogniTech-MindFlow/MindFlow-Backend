using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.model.commands;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.iam.application.services;

public interface IUserCommandService
{
    Task<Result<User>> Handle(SignUpCommand command);
    Task<Result<(User User, string Token)>> Handle(SignInCommand command);
    Task<Result<User>> Handle(UpdateProfileCommand command);
    Task<Result> Handle(DeleteAccountCommand command);
    Task<Result<(User User, string Token)>> Handle(GoogleAuthCommand command);
    Task<Result> Handle(ForgotPasswordCommand command);
    Task<Result> Handle(ResetPasswordCommand command);
    Task<Result> Handle(SetPinCommand command);
    Task<Result<bool>> Handle(VerifyPinCommand command);
    Task<Result> Handle(RemovePinCommand command);
    Task<bool> HasPinAsync(int userId);
    Task<Result<User>> GetProfileAsync(int userId);
}
