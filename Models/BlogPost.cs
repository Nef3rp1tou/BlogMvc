using Microsoft.AspNetCore.Identity;

namespace BlogMvc.Models;
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; } = DateTime.Now;
    public string UserId { get; set; } = string.Empty;

    public IdentityUser? User { get; set; }
}
