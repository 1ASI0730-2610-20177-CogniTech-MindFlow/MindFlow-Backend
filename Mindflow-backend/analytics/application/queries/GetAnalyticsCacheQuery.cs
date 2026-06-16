using Cortex.Mediator;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Shared.Domain.Model.Errors;

namespace Mindflow_backend.Analytics.Application.Queries;

public class GetAnalyticsCacheQuery : IRequest<Result<List<AnalyticsCacheDto>>>
{
    public int UserId { get; set; }
}