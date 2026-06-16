using Cortex.Mediator;
using Mindflow_backend.Analytics.Application.Dtos;
using Mindflow_backend.Shared.Domain.Model.Errors;

namespace Mindflow_backend.Analytics.Application.Commands;

public class CreateWordCloudCommand : IRequest<Result<WordCloudDto>>
{
    public int UserId { get; set; }
    public string? Words { get; set; }
}