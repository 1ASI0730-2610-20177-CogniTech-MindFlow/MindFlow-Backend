using Cortex.Mediator.Requests;
using Mindflow_backend.Journal.Application.Commands;
using Mindflow_backend.Journal.Application.Dtos;
using Mindflow_backend.Journal.Domain.Entities;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Repositories;

namespace Mindflow_backend.Journal.Application.Handlers;

public class CreateMediaHandler(
    IBaseRepository<Media> repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateMediaCommand, Result<MediaDto>>
{
    public async Task<Result<MediaDto>> Handle(CreateMediaCommand request, CancellationToken ct)
    {
        var media = new Media
        {
            EntryId = request.EntryId,
            Type = request.Type,
            Url = request.Url
        };

        await repository.AddAsync(media, ct);
        await unitOfWork.CompleteAsync(ct);

        return Result<MediaDto>.Success(Map(media));
    }

    private static MediaDto Map(Media m) => new()
    {
        Id = m.Id,
        EntryId = m.EntryId,
        Type = m.Type,
        Url = m.Url,
        CreatedAt = m.CreatedAt
    };
}