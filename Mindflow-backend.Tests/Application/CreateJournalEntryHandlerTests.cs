using Moq;
using Mindflow_backend.Analytics.Application.Services;
using Mindflow_backend.Journal.Application.Commands;
using Mindflow_backend.Journal.Application.Handlers;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Tests.Application;

public class CreateJournalEntryHandlerTests
{
    private readonly Mock<IBaseRepository<JournalEntry>> _repo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IAnalyticsCacheInvalidator> _cacheInvalidator = new();

    private CreateJournalEntryHandler CreateHandler() =>
        new(_repo.Object, _unitOfWork.Object, _cacheInvalidator.Object);

    [Fact]
    public async Task Handle_WithPositiveContent_DetectsPositiveSentiment()
    {
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
    public async Task Handle_WithAutoSentiment_DetectsFromContent()
    {
        var handler = CreateHandler();

        var command = new CreateJournalEntryCommand
        {
            UserId = 1, Date = DateOnly.FromDateTime(DateTime.Today),
            Title = "Entry", Content = "Estoy ansioso y estresado",
            Sentiment = "auto",
            Category = "Personal"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("negative", result.Value!.Sentiment);
    }

    [Fact]
    public async Task Handle_NoAiResponse_SetsNull()
    {
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
