namespace BlogMvc.Models.ViewModels;
public class BlogPostViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool CanEdit { get; set; } = false;
    public bool CanDelete { get; set; } = false;
}