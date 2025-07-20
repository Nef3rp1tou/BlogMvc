namespace BlogMvc.DTOs;
public record BlogPostResponseDto(int Id, DateTime CreatedAt, DateTime? UpdatedAt)
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
}
