namespace Mindflow_backend.iam.interfaces.rest.resources;

// Lo que el usuario envía desde el frontend o Postman
public record SignUpResource(string Email, string Password);