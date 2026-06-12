using Mindflow_backend.Shared.Infrastructure.Interfaces.AspNetCore.Configuration.Extensions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Mindflow_backend.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;

/// <summary>
///     This class is used to replace the default route naming convention with a kebab-case naming convention.
/// </summary>
public class KebabCaseRouteNamingConvention : IControllerModelConvention
{
    /// <summary>
    ///     This method applies the kebab-case naming convention to the controller.
    /// </summary>
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);

        foreach (var selector in controller.Actions.SelectMany(a => a.Selectors))
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);
    }

    private static AttributeRouteModel? ReplaceControllerTemplate(SelectorModel selector, string name)
    {
        return selector.AttributeRouteModel != null
            ? new AttributeRouteModel
            {
                Template = selector.AttributeRouteModel.Template?.Replace("[controller]", name.ToKebabCase())
            }
            : null;
    }
}
