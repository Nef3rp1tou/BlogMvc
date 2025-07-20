using BlogMvc.Extensions;
using BlogMvc.Models;
using BlogMvc.Models.Common;
using BlogMvc.Models.ViewModels;
using BlogMvc.Repositories.Interfaces;
using BlogMvc.Services.Interfaces;
using BlogMvc.Validators;
using Microsoft.AspNetCore.Identity;

namespace BlogMvc.Services;

public class BlogService : IBlogService
{
    private readonly IBlogRepository _repository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly CreatePostViewModelValidator _createValidator;
    private readonly EditPostViewModelValidator _editValidator;

    public BlogService(
        IBlogRepository repository,
        UserManager<IdentityUser> userManager,
        CreatePostViewModelValidator createValidator,
        EditPostViewModelValidator editValidator)
    {
        _repository = repository;
        _userManager = userManager;
        _createValidator = createValidator;
        _editValidator = editValidator;
    }

    // Query operations
    public async Task<Result<IEnumerable<BlogPostViewModel>>> GetAllPostsAsync()
    {
        var result = await _repository.GetAllAsync();

        if (!result.IsSuccess)
        {
            return Result<IEnumerable<BlogPostViewModel>>.Failure(result.Error!);
        }

        var viewModels = result.Value!
            .OrderByDescending(p => p.PublishedDate)
            .Select(p => MapToViewModel(p))
            .ToList();

        return Result<IEnumerable<BlogPostViewModel>>.Success(viewModels);
    }

    public async Task<Result<BlogPostViewModel>> GetPostByIdAsync(int id)
    {
        var result = await _repository.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            return Result<BlogPostViewModel>.Failure(result.Error!);
        }

        var viewModel = MapToViewModel(result.Value!);
        return Result<BlogPostViewModel>.Success(viewModel);
    }

    public async Task<Result<IEnumerable<BlogPostViewModel>>> SearchPostsByTitleAsync(string searchTerm)
    {
        var result = await _repository.GetAllAsync();

        if (!result.IsSuccess)
        {
            return Result<IEnumerable<BlogPostViewModel>>.Failure(result.Error!);
        }

        var posts = result.Value!;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            posts = posts.Where(p => p.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        var viewModels = posts
            .OrderByDescending(p => p.PublishedDate)
            .Select(p => MapToViewModel(p))
            .ToList();

        return Result<IEnumerable<BlogPostViewModel>>.Success(viewModels);
    }

    public async Task<Result<IEnumerable<BlogPostViewModel>>> GetPostsByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<IEnumerable<BlogPostViewModel>>.Failure(
                Error.Validation("User ID cannot be empty"));
        }

        var result = await _repository.GetAllAsync();

        if (!result.IsSuccess)
        {
            return Result<IEnumerable<BlogPostViewModel>>.Failure(result.Error!);
        }

        var viewModels = result.Value!
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PublishedDate)
            .Select(p => MapToViewModel(p))
            .ToList();

        return Result<IEnumerable<BlogPostViewModel>>.Success(viewModels);
    }

    public async Task<Result<IEnumerable<BlogPostViewModel>>> GetRecentPostsAsync(int count = 10)
    {
        if (count <= 0)
        {
            return Result<IEnumerable<BlogPostViewModel>>.Failure(
                Error.Validation("Count must be greater than zero"));
        }

        var result = await _repository.GetAllAsync();

        if (!result.IsSuccess)
        {
            return Result<IEnumerable<BlogPostViewModel>>.Failure(result.Error!);
        }

        var viewModels = result.Value!
            .OrderByDescending(p => p.PublishedDate)
            .Take(count)
            .Select(p => MapToViewModel(p))
            .ToList();

        return Result<IEnumerable<BlogPostViewModel>>.Success(viewModels);
    }

    // Command operations
    public async Task<Result<BlogPostViewModel>> CreatePostAsync(CreatePostViewModel model, string userId)
    {
        // Validate that userId is provided (only logged-in users can create posts)
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<BlogPostViewModel>.Failure(
                Error.Unauthorized("You must be logged in to create a post"));
        }

        // Validate input
        var validationResult = await _createValidator.ValidateAndReturnAsync(model);
        if (!validationResult.IsSuccess)
        {
            return Result<BlogPostViewModel>.Failure(validationResult.Error!);
        }

        // Create blog post directly without user verification
        // The userId comes from the authenticated user in the controller
        var blogPost = new BlogPost
        {
            Title = model.Title.Trim(),
            Content = model.Content.Trim(),
            Author = model.Author.Trim(),
            UserId = userId,
            PublishedDate = DateTime.Now
        };

        var createResult = await _repository.CreateAsync(blogPost);
        if (!createResult.IsSuccess)
        {
            return Result<BlogPostViewModel>.Failure(createResult.Error!);
        }

        var viewModel = await MapToViewModelWithPermissionsAsync(createResult.Value!, userId);
        return Result<BlogPostViewModel>.Success(viewModel);
    }

    public async Task<Result<BlogPostViewModel>> UpdatePostAsync(EditPostViewModel model, string currentUserId)
    {
        // Validate input
        var validationResult = await _editValidator.ValidateAndReturnAsync(model);
        if (!validationResult.IsSuccess)
        {
            return Result<BlogPostViewModel>.Failure(validationResult.Error!);
        }

        // Check if post exists
        var existingResult = await _repository.GetByIdAsync(model.Id);
        if (!existingResult.IsSuccess)
        {
            return Result<BlogPostViewModel>.Failure(existingResult.Error!);
        }

        var existingPost = existingResult.Value!;

        // Check permissions
        var canEditResult = await CanUserEditPostAsync(model.Id, currentUserId);
        if (!canEditResult.IsSuccess || !canEditResult.Value)
        {
            return Result<BlogPostViewModel>.Failure(
                Error.Forbidden("You don't have permission to edit this post"));
        }

        // Update the post
        existingPost.Title = model.Title.Trim();
        existingPost.Content = model.Content.Trim();
        existingPost.Author = model.Author.Trim();
        // Keep original PublishedDate and UserId

        var updateResult = await _repository.UpdateAsync(existingPost);
        if (!updateResult.IsSuccess)
        {
            return Result<BlogPostViewModel>.Failure(updateResult.Error!);
        }

        var viewModel = MapToViewModel(updateResult.Value!, currentUserId);
        return Result<BlogPostViewModel>.Success(viewModel);
    }

    public async Task<Result> DeletePostAsync(int id, string currentUserId)
    {
        // Check if post exists
        var existingResult = await _repository.GetByIdAsync(id);
        if (!existingResult.IsSuccess)
        {
            return Result.Failure(existingResult.Error!);
        }

        // Check permissions
        var canDeleteResult = await CanUserDeletePostAsync(id, currentUserId);
        if (!canDeleteResult.IsSuccess || !canDeleteResult.Value)
        {
            return Result.Failure(
                Error.Forbidden("You don't have permission to delete this post"));
        }

        return await _repository.DeleteAsync(id);
    }

    // Business logic operations
    public async Task<Result<bool>> CanUserEditPostAsync(int postId, string userId)
    {
        var postResult = await _repository.GetByIdAsync(postId);
        if (!postResult.IsSuccess)
        {
            return Result<bool>.Failure(postResult.Error!);
        }

        var post = postResult.Value!;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result<bool>.Success(false);
        }

        // Check if user is admin
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin)
        {
            return Result<bool>.Success(true);
        }

        // Check if user owns the post
        var isOwner = post.UserId == userId;
        return Result<bool>.Success(isOwner);
    }

    public async Task<Result<bool>> CanUserDeletePostAsync(int postId, string userId)
    {
        // For now, same logic as edit. Could be different in the future.
        return await CanUserEditPostAsync(postId, userId);
    }

    public async Task<Result<int>> GetPostCountByUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<int>.Failure(Error.Validation("User ID cannot be empty"));
        }

        var result = await _repository.GetAllAsync();
        if (!result.IsSuccess)
        {
            return Result<int>.Failure(result.Error!);
        }

        var count = result.Value!.Count(p => p.UserId == userId);
        return Result<int>.Success(count);
    }

    // Private helper methods
    private BlogPostViewModel MapToViewModel(BlogPost post, string? currentUserId = null)
    {
        var viewModel = new BlogPostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Author = post.Author,
            PublishedDate = post.PublishedDate,
            UserId = post.UserId,
            CanEdit = false,
            CanDelete = false
        };

        // If no user is logged in (guest), they can't edit or delete
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return viewModel;
        }

        // Set permissions asynchronously would be better, but for now we'll handle it in the calling methods
        return viewModel;
    }

    private async Task<BlogPostViewModel> MapToViewModelWithPermissionsAsync(BlogPost post, string? currentUserId)
    {
        var viewModel = MapToViewModel(post, currentUserId);

        if (!string.IsNullOrWhiteSpace(currentUserId))
        {
            var canEditResult = await CanUserEditPostAsync(post.Id, currentUserId);
            viewModel.CanEdit = canEditResult.IsSuccess && canEditResult.Value;

            var canDeleteResult = await CanUserDeletePostAsync(post.Id, currentUserId);
            viewModel.CanDelete = canDeleteResult.IsSuccess && canDeleteResult.Value;
        }

        return viewModel;
    }

    // Authorization helpers
    public async Task<Result<BlogPostViewModel>> GetPostWithPermissionsAsync(int id, string? currentUserId)
    {
        var result = await _repository.GetByIdAsync(id);

        if (!result.IsSuccess)
        {
            return Result<BlogPostViewModel>.Failure(result.Error!);
        }

        var viewModel = await MapToViewModelWithPermissionsAsync(result.Value!, currentUserId);
        return Result<BlogPostViewModel>.Success(viewModel);
    }

    public async Task<Result<IEnumerable<BlogPostViewModel>>> GetAllPostsWithPermissionsAsync(string? currentUserId)
    {
        var result = await _repository.GetAllAsync();

        if (!result.IsSuccess)
        {
            return Result<IEnumerable<BlogPostViewModel>>.Failure(result.Error!);
        }

        var viewModels = new List<BlogPostViewModel>();
        foreach (var post in result.Value!.OrderByDescending(p => p.PublishedDate))
        {
            var viewModel = await MapToViewModelWithPermissionsAsync(post, currentUserId);
            viewModels.Add(viewModel);
        }

        return Result<IEnumerable<BlogPostViewModel>>.Success(viewModels);
    }
}