using Microsoft.AspNetCore.Mvc;
using Mindflow_backend.iam.application.services;
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
        // 1. Transformar el JSON a un Comando del Dominio
        var command = SignUpCommandFromResourceAssembler.ToCommandFromResource(resource);
        
        // 2. Ejecutar el caso de uso
        var result = await userCommandService.Handle(command);

        // 3. Evaluar el resultado usando la clase Result de tu equipo
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Message });
        }

        // 4. Transformar la entidad guardada a un JSON seguro
        var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(result.Value!);
        
        return CreatedAtAction(nameof(SignUp), new { id = userResource.Id }, userResource);
    }
}