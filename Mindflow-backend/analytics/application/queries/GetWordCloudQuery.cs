using Cortex.Mediator;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Model;

namespace Mindflow_backend.Analytics.Application.Queries;

public class GetWordCloudQuery : IRequest<Result<WordCloudDto>>
{
    public int UserId { get; set; }
}