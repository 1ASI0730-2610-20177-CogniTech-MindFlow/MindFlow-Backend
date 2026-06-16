using Mindflow_backend.WellnessEngine.Application.Dtos;

namespace Mindflow_backend.WellnessEngine.Application.Services;

public interface IWellnessService
{
    Task<StressCheckResultDto> RunStressCheckAsync(int userId, CancellationToken ct = default);
}
