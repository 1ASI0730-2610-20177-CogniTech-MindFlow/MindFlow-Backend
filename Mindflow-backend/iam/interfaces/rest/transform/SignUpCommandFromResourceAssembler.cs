using Mindflow_backend.iam.domain.model.commands;
using Mindflow_backend.iam.interfaces.rest.resources;

namespace Mindflow_backend.iam.interfaces.rest.transform;

public static class SignUpCommandFromResourceAssembler
{
    public static SignUpCommand ToCommandFromResource(SignUpResource resource)
    {
        return new SignUpCommand(resource.Email, resource.Password, resource.Name);
    }
}