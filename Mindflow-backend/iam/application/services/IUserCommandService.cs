using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.model.commands;
using Mindflow_backend.Shared.Application.Model;

namespace Mindflow_backend.iam.application.services;

public interface IUserCommandService
{
    Task<Result<User>> Handle(SignUpCommand command);
    Task<Result<(User User, string Token)>> Handle(SignInCommand command);
}
