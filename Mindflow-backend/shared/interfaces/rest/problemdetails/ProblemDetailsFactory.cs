using Mindflow_backend.Resources.Errors;
using Mindflow_backend.Resources.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Mindflow_backend.Shared.Interfaces.Rest.ProblemDetails;

public class ProblemDetailsFactory
{
    private readonly Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory _aspNetCoreProblemDetailsFactory;
    private readonly IStringLocalizer<CommonMessages> _commonLocalizer;
    private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;

    public ProblemDetailsFactory(
        IStringLocalizer<ErrorMessages> errorLocalizer,
        IStringLocalizer<CommonMessages> commonLocalizer,
        Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory aspNetCoreProblemDetailsFactory)
    {
        _errorLocalizer = errorLocalizer;
        _commonLocalizer = commonLocalizer;
        _aspNetCoreProblemDetailsFactory = aspNetCoreProblemDetailsFactory;
    }

    /// <summary>
    ///     Creates a ProblemDetails response using an error enum and a detail message.
    /// </summary>
    public IActionResult CreateProblemDetails(
        ControllerBase controller,
        int statusCode,
        Enum? errorEnum,
        string detailMessage)
    {
        var problemDetails = _aspNetCoreProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode,
            errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"],
            detail: detailMessage
        );

        if (problemDetails == null)
        {
            problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = statusCode,
                Title = errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"],
                Detail = detailMessage,
                Instance = controller.HttpContext.Request.Path
            };
        }
        else
        {
            problemDetails.Title =
                errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"];
            problemDetails.Detail = detailMessage;
            problemDetails.Instance = controller.HttpContext.Request.Path;
        }

        return controller.StatusCode(statusCode, problemDetails);
    }

    /// <summary>
    ///     Creates a ProblemDetails response using localization keys.
    /// </summary>
    public IActionResult CreateProblemDetails(
        ControllerBase controller,
        int statusCode,
        string titleKey,
        string detailKey,
        params object[] detailArgs)
    {
        var problemDetails = _aspNetCoreProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode,
            _commonLocalizer[titleKey],
            detail: _errorLocalizer[detailKey, detailArgs],
            instance: controller.HttpContext.Request.Path
        );
        return controller.StatusCode(statusCode, problemDetails);
    }
}
