namespace Mindflow_backend.iam.interfaces.rest.resources;

// Lo que la API le responde al usuario (sin el password)
public record UserResource(int Id, string Email);