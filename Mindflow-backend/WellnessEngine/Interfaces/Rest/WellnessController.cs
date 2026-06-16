using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.WellnessEngine.Application.Services;

namespace Mindflow_backend.WellnessEngine.Interfaces.Rest;

[ApiController]
[Route("wellness")]
[Authorize]
public sealed class WellnessController(IWellnessService wellnessService) : ControllerBase
{
    [HttpPost("stress-check")]
    public async Task<IActionResult> StressCheck(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var result = await wellnessService.RunStressCheckAsync(userId, ct);
        return Ok(result);
    }
}
