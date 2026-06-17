using Moq;
using Mindflow_backend.AiIntegration.Application.Services;
using Mindflow_backend.Journal.Application.Commands;
using Mindflow_backend.Journal.Application.Handlers;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Tests.Application;

public class CreateJournalEntryHandlerTests
{
    private readonly Mock<IBaseRepository<JournalEntry>> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAiService> _aiService = new();

    private CreateJournalEntryHandler CreateHandler() =>
        new(_repo.Object, _unitOfWork.Object, _aiService.Object);

    [Fact]
    public async Task Handle_WithPositiveContent_DetectsPositiveSentiment()
    {
        _aiService.Setup(a => a.GenerateEmpathicResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Great job!");
        var handler = CreateHandler();

        var command = new CreateJournalEntryCommand
        {
            UserId = 1, Date = DateOnly.FromDateTime(DateTime.Today),
            Title = "Good day", Content = "Me siento feliz y genial hoy",
            Sentiment = "", Category = "Personal"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("positive", result.Value!.Sentiment);
    }

    [Fact]
    public async Task Handle_WithNegativeContent_DetectsNegativeSentiment()
    {
        _aiService.Setup(a => a.GenerateEmpathicResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("");
        var handler = CreateHandler();

        var command = new CreateJournalEntryCommand
        {
            UserId = 1, Date = DateOnly.FromDateTime(DateTime.Today),
            Title = "Bad day", Content = "Estoy muy triste y frustrado",
            Sentiment = "", Category = "Personal"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("negative", result.Value!.Sentiment);
    }

    [Fact]
    public async Task Handle_WithNeutralContent_DetectsNeutralSentiment()
    {
        _aiService.Setup(a => a.GenerateEmpathicResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("");
        var handler = CreateHandler();

        var command = new CreateJournalEntryCommand
        {
            UserId = 1, Date = DateOnly.FromDateTime(DateTime.Today),
            Title = "Regular day", Content = "Hoy tuve una reunión de trabajo",
            Sentiment = "", Category = "Work"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("neutral", result.Value!.Sentiment);
    }

    [Fact]
    public async Task Handle_WithExplicitSentiment_UsesProvidedSentiment()
    {
        _aiService.Setup(a => a.GenerateEmpathicResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("");
        var handler = CreateHandler();

        var command = new CreateJournalEntryCommand
        {
            UserId = 1, Date = DateOnly.FromDateTime(DateTime.Today),
            Title = "Test", Content = "Estoy muy triste",
            Sentiment = "positive",
            Category = "Personal"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("positive", result.Value!.Sentiment);
    }

    [Fact]
    public async Task Handle_WithAiResponse_IncludesItInResult()
    {
        _aiService.Setup(a => a.GenerateEmpathicResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Te entiendo, es normal sentirse así.");
        var handler = CreateHandler();

        var command = new CreateJournalEntryCommand
        {
            UserId = 1, Date = DateOnly.FromDateTime(DateTime.Today),
            Title = "Entry", Content = "Estoy ansioso",
            Category = "Personal"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Te entiendo, es normal sentirse así.", result.Value!.AiResponse);
    }

    [Fact]
    public async Task Handle_WithEmptyAiResponse_SetsNull()
    {
        _aiService.Setup(a => a.GenerateEmpathicResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("");
        var handler = CreateHandler();

        var command = new CreateJournalEntryCommand
        {
            UserId = 1, Date = DateOnly.FromDateTime(DateTime.Today),
            Title = "Entry", Content = "Test content",
            Category = "Personal"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Null(result.Value!.AiResponse);
    }

    [Fact]
    public async Task Handle_WithLongContent_SetsHasPreviewTrue()
    {
        _aiService.Setup(a => a.GenerateEmpathicResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("");
        var handler = CreateHandler();

        var command = new CreateJournalEntryCommand
        {
            UserId = 1, Date = DateOnly.FromDateTime(DateTime.Today),
            Title = "Long", Content = new string('a', 250),
            Category = "Personal"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.Value!.HasPreview);
    }

    [Fact]
    public async Task Handle_WithShortContent_SetsHasPreviewFalse()
    {
        _aiService.Setup(a => a.GenerateEmpathicResponseAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("");
        var handler = CreateHandler();

        var command = new CreateJournalEntryCommand
        {
            UserId = 1, Date = DateOnly.FromDateTime(DateTime.Today),
            Title = "Short", Content = "Brief text",
            Category = "Personal"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.Value!.HasPreview);
    }
}
