namespace Mindflow_backend.Analytics.Application.Services;

public interface IAnalyticsCacheInvalidator
{
    Task InvalidateAsync(int userId, CancellationToken ct = default);
}
