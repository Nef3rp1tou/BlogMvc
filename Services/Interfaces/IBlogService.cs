using BlogMvc.Models;
using BlogMvc.Models.Common;
using BlogMvc.Models.ViewModels;

namespace BlogMvc.Services.Interfaces;

public interface IBlogService
{
    // Query operations
    Task<Result<IEnumerable<BlogPostViewModel>>> GetAllPostsAsync();
    Task<Result<BlogPostViewModel>> GetPostByIdAsync(int id);
    Task<Result<IEnumerable<BlogPostViewModel>>> SearchPostsByTitleAsync(string searchTerm);
    Task<Result<IEnumerable<BlogPostViewModel>>> GetPostsByUserIdAsync(string userId);
    Task<Result<IEnumerable<BlogPostViewModel>>> GetRecentPostsAsync(int count = 10);

    // Command operations
    Task<Result<BlogPostViewModel>> CreatePostAsync(CreatePostViewModel model, string userId);
    Task<Result<BlogPostViewModel>> UpdatePostAsync(EditPostViewModel model, string currentUserId);
    Task<Result> DeletePostAsync(int id, string currentUserId);

    // Business logic operations
    Task<Result<bool>> CanUserEditPostAsync(int postId, string userId);
    Task<Result<bool>> CanUserDeletePostAsync(int postId, string userId);
    Task<Result<int>> GetPostCountByUserAsync(string userId);

    // Authorization helpers
    Task<Result<BlogPostViewModel>> GetPostWithPermissionsAsync(int id, string? currentUserId);
    Task<Result<IEnumerable<BlogPostViewModel>>> GetAllPostsWithPermissionsAsync(string? currentUserId);
}