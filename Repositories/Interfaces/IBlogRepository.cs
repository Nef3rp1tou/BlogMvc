using BlogMvc.Models;
using BlogMvc.Models.Common;

namespace BlogMvc.Repositories.Interfaces;
public interface IBlogRepository
{
    Task<Result<IEnumerable<BlogPost>>> GetAllAsync();
    Task<Result<BlogPost>> GetByIdAsync(int id);
    Task<Result<BlogPost>> CreateAsync(BlogPost blogPost);
    Task<Result<BlogPost>> UpdateAsync(BlogPost blogPost);
    Task<Result> DeleteAsync(int id);
    Task<Result<bool>> ExistsAsync(int id);
}