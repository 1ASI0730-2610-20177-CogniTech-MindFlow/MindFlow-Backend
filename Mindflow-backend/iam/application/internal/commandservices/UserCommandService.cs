using Mindflow_backend.iam.application.errors;
using Mindflow_backend.iam.application.services;
using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.model.commands;
using Mindflow_backend.iam.domain.repositories;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.iam.application.Internal.commandservices;

public class UserCommandService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService) : IUserCommandService
{
    public async Task<Result<User>> Handle(SignUpCommand command)
    {
        if (userRepository.ExistsByEmail(command.Email))
            return Result<User>.Failure(SignUpError.EmailAlreadyInUse, "El correo ingresado ya está en uso.");

        try
        {
            var user = new User(command.Email, command.Password);
            await userRepository.AddAsync(user);
            await unitOfWork.CompleteAsync();
            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(SignUpError.UnexpectedError, $"Ocurrió un error: {ex.Message}");
        }
    }

    public async Task<Result<(User User, string Token)>> Handle(SignInCommand command)
    {
        var user = await userRepository.FindByEmailAsync(command.Email);
        if (user == null || user.PasswordHash != command.Password)
            return Result<(User, string)>.Failure(SignInError.InvalidCredentials, "Email o contraseña incorrectos.");

        var token = tokenService.GenerateToken(user);
        return Result<(User, string)>.Success((user, token));
    }
}
