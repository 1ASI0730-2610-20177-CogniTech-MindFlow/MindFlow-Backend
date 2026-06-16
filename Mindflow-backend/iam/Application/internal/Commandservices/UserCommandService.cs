using Mindflow_backend.iam.application.errors;
using Mindflow_backend.iam.application.services;
using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.model.commands;
using Mindflow_backend.iam.domain.repositories;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.iam.Application.Internal.Commandservices;

public class UserCommandService(
    IUserRepository userRepository, 
    IUnitOfWork unitOfWork) : IUserCommandService
{
    public async Task<Result<User>> Handle(SignUpCommand command)
    {
        // 1. Validar si el correo ya existe
        if (userRepository.ExistsByEmail(command.Email))
        {
            // Usamos la sintaxis de tu Result: Failure(Enum, mensaje)
            return Result<User>.Failure(SignUpError.EmailAlreadyInUse, "El correo ingresado ya está en uso.");
        }

        try
        {
            var hashedPassword = command.Password; // TODO: Hashing real luego
            
            var user = new User(command.Email, hashedPassword);
            await userRepository.AddAsync(user);
            await unitOfWork.CompleteAsync();

            // Usamos la sintaxis de tu Result: Success(valor)
            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(SignUpError.UnexpectedError, $"Ocurrió un error: {ex.Message}");
        }
    }
}