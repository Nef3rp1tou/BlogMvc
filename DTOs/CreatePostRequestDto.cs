namespace BlogMvc.DTOs;
public class CreatePostRequestDto
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string Author { get; set; } = null!;
}
