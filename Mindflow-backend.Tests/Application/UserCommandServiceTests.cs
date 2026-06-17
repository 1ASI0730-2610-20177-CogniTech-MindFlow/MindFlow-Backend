using Moq;
using Mindflow_backend.iam.application.Internal.commandservices;
using Mindflow_backend.iam.application.services;
using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.domain.model.commands;
using Mindflow_backend.iam.domain.repositories;
using Mindflow_backend.Shared.Domain.Repositories;
using Mindflow_backend.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace Mindflow_backend.Tests.Application;

public class UserCommandServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IGoogleAuthService> _googleAuth = new();
    private readonly Mock<IEmailService> _emailService = new();

    private UserCommandService CreateService(AppDbContext? dbContext = null) =>
        new(_userRepo.Object, _unitOfWork.Object, _tokenService.Object,
            dbContext!, _googleAuth.Object, _emailService.Object);

    [Fact]
    public async Task SignUp_WithNewEmail_ReturnsSuccess()
    {
        _userRepo.Setup(r => r.ExistsByEmail("new@mail.com")).Returns(false);
        var service = CreateService();

        var result = await service.Handle(new SignUpCommand("new@mail.com", "Pass123"));

        Assert.True(result.IsSuccess);
        Assert.Equal("new@mail.com", result.Value!.Email);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(default), Times.Once);
    }

    [Fact]
    public async Task SignUp_WithExistingEmail_ReturnsFailure()
    {
        _userRepo.Setup(r => r.ExistsByEmail("exists@mail.com")).Returns(true);
        var service = CreateService();

        var result = await service.Handle(new SignUpCommand("exists@mail.com", "Pass123"));

        Assert.True(result.IsFailure);
        Assert.Contains("ya está en uso", result.Message);
    }

    [Fact]
    public async Task SignIn_WithValidCredentials_ReturnsToken()
    {
        var user = new User("test@mail.com", "CorrectPass");
        _userRepo.Setup(r => r.FindByEmailAsync("test@mail.com")).ReturnsAsync(user);
        _tokenService.Setup(t => t.GenerateToken(user)).Returns("jwt-token-123");
        var service = CreateService();

        var result = await service.Handle(new SignInCommand("test@mail.com", "CorrectPass"));

        Assert.True(result.IsSuccess);
        Assert.Equal("jwt-token-123", result.Value.Token);
        Assert.Equal(user, result.Value.User);
    }

    [Fact]
    public async Task SignIn_WithWrongPassword_ReturnsFailure()
    {
        var user = new User("test@mail.com", "CorrectPass");
        _userRepo.Setup(r => r.FindByEmailAsync("test@mail.com")).ReturnsAsync(user);
        var service = CreateService();

        var result = await service.Handle(new SignInCommand("test@mail.com", "WrongPass"));

        Assert.True(result.IsFailure);
        Assert.Contains("incorrectos", result.Message);
    }

    [Fact]
    public async Task SignIn_WithNonexistentEmail_ReturnsFailure()
    {
        _userRepo.Setup(r => r.FindByEmailAsync("nobody@mail.com")).ReturnsAsync((User?)null);
        var service = CreateService();

        var result = await service.Handle(new SignInCommand("nobody@mail.com", "Pass"));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SetPin_WithValidPin_ReturnsSuccess()
    {
        var user = new User("test@mail.com", "pass");
        _userRepo.Setup(r => r.FindByIdAsync(1, default)).ReturnsAsync(user);
        var service = CreateService();

        var result = await service.Handle(new SetPinCommand(1, "1234"));

        Assert.True(result.IsSuccess);
        Assert.True(user.HasPin);
    }

    [Fact]
    public async Task SetPin_WithInvalidPin_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.Handle(new SetPinCommand(1, "abc"));
        Assert.True(result.IsFailure);
        Assert.Contains("numérico", result.Message);

        var result2 = await service.Handle(new SetPinCommand(1, "12"));
        Assert.True(result2.IsFailure);

        var result3 = await service.Handle(new SetPinCommand(1, "1234567"));
        Assert.True(result3.IsFailure);
    }

    [Fact]
    public async Task VerifyPin_WithCorrectPin_ReturnsTrue()
    {
        var user = new User("test@mail.com", "pass");
        user.SetPin("9999");
        _userRepo.Setup(r => r.FindByIdAsync(1, default)).ReturnsAsync(user);
        var service = CreateService();

        var result = await service.Handle(new VerifyPinCommand(1, "9999"));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task VerifyPin_WithWrongPin_ReturnsFalse()
    {
        var user = new User("test@mail.com", "pass");
        user.SetPin("9999");
        _userRepo.Setup(r => r.FindByIdAsync(1, default)).ReturnsAsync(user);
        var service = CreateService();

        var result = await service.Handle(new VerifyPinCommand(1, "0000"));

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task UpdateProfile_SetsNameAndOccupation()
    {
        var user = new User("test@mail.com", "pass");
        _userRepo.Setup(r => r.FindByIdAsync(1, default)).ReturnsAsync(user);
        var service = CreateService();

        var result = await service.Handle(new UpdateProfileCommand(1, "Alice", "Dev"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Alice", result.Value!.Name);
        Assert.Equal("Dev", result.Value.Occupation);
    }
}
