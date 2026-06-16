using Mindflow_backend.Shared.Application.Model;
using Mindflow_backend.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Mindflow_backend.Habits.Interfaces.Rest.Assemblers;

public static class HabitsActionResultAssembler
{
    public static IActionResult ToActionResultFromCreateResult<T>(
        ControllerBase controller,
        Result<T> result,
        ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> successAction)
    {
        if (result.IsSuccess)
            return successAction(result.Value!);

        var statusCode = result.Error switch
        {
            var e when e?.ToString()?.Contains("NotFound") ?? false => 404,
            var e when e?.ToString()?.Contains("Mismatch") ?? false => 403,
            _ => 400
        };

        return problemDetailsFactory.CreateProblemDetails(controller, statusCode, result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromDeleteResult(
        ControllerBase controller,
        Result result,
        ProblemDetailsFactory problemDetailsFactory)
    {
        if (result.IsSuccess)
            return controller.NoContent();

        var statusCode = result.Error?.ToString()?.Contains("NotFound") ?? false ? 404 : 400;
        return problemDetailsFactory.CreateProblemDetails(controller, statusCode, result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromGetResult<T>(
        ControllerBase controller,
        T? entity,
        ProblemDetailsFactory problemDetailsFactory,
        Func<T, IActionResult> successAction)
    {
        if (entity != null)
            return successAction(entity);

        return problemDetailsFactory.CreateProblemDetails(controller, 404, (Enum?)null, "Resource not found.");
    }

    public static IActionResult ToActionResultFromGetAllResult<T>(
        ControllerBase controller,
        IEnumerable<T> entities,
        Func<IEnumerable<T>, IActionResult> successAction)
    {
        return successAction(entities);
    }
}
