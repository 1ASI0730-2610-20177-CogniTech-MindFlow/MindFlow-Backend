using Mindflow_backend.iam.domain.model.aggregates;
using Mindflow_backend.iam.interfaces.rest.resources;

namespace Mindflow_backend.iam.interfaces.rest.transform;

public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User entity)
    {
        return new UserResource(entity.Id, entity.Email, entity.Name, entity.Occupation);
    }
}