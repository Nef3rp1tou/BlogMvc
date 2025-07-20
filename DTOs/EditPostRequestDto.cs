namespace BlogMvc.DTOs;
public record EditPostRequestDto(int Id)
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Author { get; set; } = null!;
}
