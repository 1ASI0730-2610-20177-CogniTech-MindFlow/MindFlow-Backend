namespace Mindflow_backend.Journal.Application.Dtos;

public class TagDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}