using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.iam.application.services;
using Mindflow_backend.iam.domain.model.commands;
using Mindflow_backend.iam.interfaces.rest.resources;
using Mindflow_backend.iam.interfaces.rest.transform;

namespace Mindflow_backend.iam.interfaces.rest;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class UsersController(IUserCommandService userCommandService) : ControllerBase
{
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource resource)
    {
        var command = SignUpCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await userCommandService.Handle(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Message });

        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(result.Value!);
        return CreatedAtAction(nameof(SignUp), new { id = userResource.Id }, userResource);
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource)
    {
        var command = new SignInCommand(resource.Email, resource.Password);
        var result = await userCommandService.Handle(command);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Message });

        var (user, token) = result.Value;
        return Ok(new AuthenticatedUserResource(user.Id, user.Email, token));
    }
}
