using BlogMvc.Data;
using BlogMvc.Models;
using BlogMvc.Models.Common;
using BlogMvc.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BlogMvc.Repositories;
public class BlogRepository(ApplicationDbContext context) : IBlogRepository
{
    private ApplicationDbContext _context = context;

    #region Read Operations
    public async Task<Result<BlogPost>> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            return Result<BlogPost>.Failure(Error.Validation("Invalid ID"));
        }
        var post = await _context.BlogPosts
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return Result<BlogPost>.Failure(Error.NotFound("BlogPost", id));
        }
        return Result<BlogPost>.Success(post);
    }
    public async Task<Result<IEnumerable<BlogPost>>> GetAllAsync()
    {
        var posts = await _context.BlogPosts
            .AsNoTracking()
            .Include(p => p.User)
            .ToListAsync();
        return Result<IEnumerable<BlogPost>>.Success(posts);
    }
    #endregion

    #region Write Operations
    public async Task<Result<BlogPost>> CreateAsync(BlogPost blogPost)
    {
        if (blogPost == null)
        {
            return Result<BlogPost>.Failure(Error.Validation("Blog post cannot be null"));
        }

        if (string.IsNullOrWhiteSpace(blogPost.Title))
        {
            return Result<BlogPost>.Failure(Error.Validation("Title is required"));
        }

        _context.BlogPosts.Add(blogPost);

        var saveResult = await SaveChangesAsync();
        if (!saveResult.IsSuccess)
        {
            return Result<BlogPost>.Failure(saveResult.Error!);
        }

        // Reload with User navigation property
        await _context.Entry(blogPost)
            .Reference(p => p.User)
            .LoadAsync();

        return Result<BlogPost>.Success(blogPost);
    }

    public async Task<Result<BlogPost>> UpdateAsync(BlogPost blogPost)
    {
        if (blogPost == null)
        {
            return Result<BlogPost>.Failure(Error.Validation("Blog post cannot be null"));
        }

        if (blogPost.Id <= 0)
        {
            return Result<BlogPost>.Failure(Error.Validation("Invalid post ID"));
        }

        if (string.IsNullOrWhiteSpace(blogPost.Title))
        {
            return Result<BlogPost>.Failure(Error.Validation("Title is required"));
        }

        _context.BlogPosts.Update(blogPost);

        var saveResult = await SaveChangesAsync();
        if (!saveResult.IsSuccess)
        {
            return Result<BlogPost>.Failure(saveResult.Error!);
        }

        // Reload with User navigation property
        await _context.Entry(blogPost)
            .Reference(p => p.User)
            .LoadAsync();

        return Result<BlogPost>.Success(blogPost);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return Result.Failure(Error.Validation("Invalid post ID"));
        }

        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null)
        {
            return Result.Failure(Error.NotFound("BlogPost", id));
        }

        _context.BlogPosts.Remove(post);

        return await SaveChangesAsync();
    }

    public async Task<Result<bool>> ExistsAsync(int id)
    {
        if (id <= 0)
        {
            return Result<bool>.Failure(Error.Validation("Invalid post ID"));
        }

        var exists = await _context.BlogPosts.AnyAsync(p => p.Id == id);
        return Result<bool>.Success(exists);
    }

    public async Task<Result<int>> GetTotalCountAsync()
    {
        var count = await _context.BlogPosts.CountAsync();
        return Result<int>.Success(count);
    }

    #endregion


    private async Task<Result> SaveChangesAsync()
    {
        var rowsAffected = await _context.SaveChangesAsync();

        if (rowsAffected > 0)
        {
            return Result.Success();
        }

        return Result.Failure(Error.InternalServerError("No changes were saved to the database"));
    }


}

