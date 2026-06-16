using Cortex.Mediator.Queries;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Domain.Model;

namespace Mindflow_backend.Analytics.Application.Queries;

public class GetAnalyticsCacheByWeekQuery : IQuery<Result<AnalyticsCacheDto>>
{
    public int UserId { get; set; }
    public DateOnly WeekStart { get; set; }
}