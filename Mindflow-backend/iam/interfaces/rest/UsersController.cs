using Microsoft.AspNetCore.Authorization;
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

    [HttpPost("google-auth")]
    public async Task<IActionResult> GoogleAuth([FromBody] GoogleAuthResource resource)
    {
        var command = new GoogleAuthCommand(resource.Credential);
        var result = await userCommandService.Handle(command);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Message });

        var (user, token) = result.Value;
        return Ok(new AuthenticatedUserResource(user.Id, user.Email, token));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordResource resource)
    {
        var command = new ForgotPasswordCommand(resource.Email);
        await userCommandService.Handle(command);
        return Ok(new { message = "Si el correo existe, recibirás un enlace de recuperación." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordResource resource)
    {
        var command = new ResetPasswordCommand(resource.Token, resource.NewPassword);
        var result = await userCommandService.Handle(command);
        if (result.IsFailure) return BadRequest(new { error = result.Message });
        return Ok(new { message = "Contraseña actualizada correctamente." });
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileResource resource)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var command = new UpdateProfileCommand(userId, resource.Name, resource.Occupation);
        var result = await userCommandService.Handle(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Message });

        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var command = new DeleteAccountCommand(userId);
        var result = await userCommandService.Handle(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Message });

        return NoContent();
    }

    [HttpPost("pin")]
    [Authorize]
    public async Task<IActionResult> SetPin([FromBody] SetPinResource resource)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var command = new SetPinCommand(userId, resource.Pin);
        var result = await userCommandService.Handle(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Message });

        return Ok(new { message = "PIN configurado correctamente." });
    }

    [HttpPost("pin/verify")]
    [Authorize]
    public async Task<IActionResult> VerifyPin([FromBody] VerifyPinResource resource)
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var command = new VerifyPinCommand(userId, resource.Pin);
        var result = await userCommandService.Handle(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Message });

        return Ok(new { valid = result.Value });
    }

    [HttpDelete("pin")]
    [Authorize]
    public async Task<IActionResult> RemovePin()
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var command = new RemovePinCommand(userId);
        var result = await userCommandService.Handle(command);

        if (result.IsFailure)
            return BadRequest(new { error = result.Message });

        return Ok(new { message = "PIN eliminado correctamente." });
    }

    [HttpGet("pin/status")]
    [Authorize]
    public async Task<IActionResult> PinStatus()
    {
        var userId = int.Parse(User.FindFirst("user_id")!.Value);
        var hasPin = await userCommandService.HasPinAsync(userId);
        return Ok(new { has_pin = hasPin });
    }
}
