using Microsoft.EntityFrameworkCore;
using Mindflow_backend.iam.application.errors;
using Mindflow_backend.iam.application.services;
using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.model.commands;
using Mindflow_backend.iam.domain.repositories;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.iam.application.Internal.commandservices;

public class UserCommandService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    AppDbContext dbContext) : IUserCommandService
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
        if (user == null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            return Result<(User, string)>.Failure(SignInError.InvalidCredentials, "Email o contraseña incorrectos.");

        var token = tokenService.GenerateToken(user);
        return Result<(User, string)>.Success((user, token));
    }

    public async Task<Result<User>> Handle(UpdateProfileCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user == null)
            return Result<User>.Failure(SignUpError.UnexpectedError, "Usuario no encontrado.");

        user.UpdateProfile(command.Name, command.Occupation);
        userRepository.Update(user);
        await unitOfWork.CompleteAsync();
        return Result<User>.Success(user);
    }

    public async Task<Result> Handle(DeleteAccountCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId);
        if (user == null)
            return Result.Failure(SignUpError.UnexpectedError, "Usuario no encontrado.");

        var entryIds = await dbContext.JournalEntries
            .IgnoreQueryFilters()
            .Where(e => e.UserId == command.UserId)
            .Select(e => e.Id)
            .ToListAsync();

        if (entryIds.Count > 0)
        {
            await dbContext.EntryTags.Where(et => entryIds.Contains(et.EntryId)).ExecuteDeleteAsync();
            await dbContext.Media.Where(m => entryIds.Contains(m.EntryId)).ExecuteDeleteAsync();
            await dbContext.JournalEntries.IgnoreQueryFilters()
                .Where(e => e.UserId == command.UserId).ExecuteDeleteAsync();
        }

        var habitIds = await dbContext.Set<Mindflow_backend.Habits.Domain.Model.Aggregates.Habit>()
            .Where(h => h.UserId == command.UserId)
            .Select(h => h.Id)
            .ToListAsync();

        if (habitIds.Count > 0)
        {
            await dbContext.Set<Mindflow_backend.Habits.Domain.Model.Entities.HabitCompletionLog>()
                .Where(l => habitIds.Contains(l.HabitId)).ExecuteDeleteAsync();
            await dbContext.Set<Mindflow_backend.Habits.Domain.Model.Aggregates.Habit>()
                .Where(h => h.UserId == command.UserId).ExecuteDeleteAsync();
        }

        await dbContext.AnalyticsCaches.Where(a => a.UserId == command.UserId).ExecuteDeleteAsync();
        await dbContext.WordClouds.Where(w => w.UserId == command.UserId).ExecuteDeleteAsync();

        userRepository.Remove(user);
        await unitOfWork.CompleteAsync();
        return Result.Success();
    }
}
