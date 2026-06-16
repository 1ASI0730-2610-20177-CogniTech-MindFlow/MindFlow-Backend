using Mindflow_backend.iam.application.errors;
using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.model.commands;
using Mindflow_backend.Shared.Application.Model; // Asumiendo que aquí está tu patrón Result

namespace Mindflow_backend.iam.application.services;

public interface IUserCommandService
{
    Task<Result<User>> Handle(SignUpCommand command);
}