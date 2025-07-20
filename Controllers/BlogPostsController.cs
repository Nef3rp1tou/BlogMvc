using BlogMvc.DTOs;
using BlogMvc.Models.Common;
using BlogMvc.Models.ViewModels;
using BlogMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogMvc.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class BlogPostController : BaseController
{
    private readonly IBlogService _blogService;
    private readonly UserManager<IdentityUser> _userManager;

    public BlogPostController(
        IBlogService blogService,
        UserManager<IdentityUser> userManager)
    {
        _blogService = blogService;
        _userManager = userManager;
    }

    #region Query Operations

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<IEnumerable<BlogPostViewModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllPosts()
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.GetAllPostsWithPermissionsAsync(userId);

        return CreateResponse(result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<BlogPostViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPostById(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.GetPostWithPermissionsAsync(id, userId);

        return CreateResponse(result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<IEnumerable<BlogPostViewModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchPosts(string searchTerm)
    {
        var result = await _blogService.SearchPostsByTitleAsync(searchTerm);

        return CreateResponse(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<BlogPostViewModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyPosts()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _blogService.GetPostsByUserIdAsync(userId);

        return CreateResponse(result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<IEnumerable<BlogPostViewModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecentPosts(int count = 10)
    {
        var result = await _blogService.GetRecentPostsAsync(count);

        return CreateResponse(result);
    }

    #endregion

    #region Command Operations

    [HttpPost]
    [ProducesResponseType(typeof(Result<BlogPostViewModel>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Verify the user exists and has proper role
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        // Check if user has permission to create posts
        var isInUserRole = await _userManager.IsInRoleAsync(user, "User");
        var isInAdminRole = await _userManager.IsInRoleAsync(user, "Admin");

        if (!isInUserRole && !isInAdminRole)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new DummyError
                {
                    Code = "FORBIDDEN",
                    Message = "You don't have permission to create posts"
                });
        }

        var viewModel = new CreatePostViewModel
        {
            Title = request.Title,
            Content = request.Content,
            Author = request.Author
        };

        var result = await _blogService.CreatePostAsync(viewModel, userId);

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetPostById),
                new { id = result.Value!.Id },
                result);
        }

        return CreateResponse(result);
    }

    [HttpPut]
    [ProducesResponseType(typeof(Result<BlogPostViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePost([FromBody] EditPostRequestDto request)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var viewModel = new EditPostViewModel
        {
            Id = request.Id,
            Title = request.Title,
            Content = request.Content,
            Author = request.Author
        };

        var result = await _blogService.UpdatePostAsync(viewModel, userId);

        return CreateResponse(result);
    }

    [HttpDelete]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePost(int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _blogService.DeletePostAsync(id, userId);

        return CreateResponse(result);
    }

    #endregion

    #region Business Operations

    [HttpGet]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CanEditPost(int postId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _blogService.CanUserEditPostAsync(postId, userId);

        return CreateResponse(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CanDeletePost(int postId)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _blogService.CanUserDeletePostAsync(postId, userId);

        return CreateResponse(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(DummyError), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserPostCount()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _blogService.GetPostCountByUserAsync(userId);

        return CreateResponse(result);
    }

    #endregion

    #region Private Methods

    private string? GetCurrentUserId()
    {
        return User.Identity?.IsAuthenticated == true
            ? _userManager.GetUserId(User)
            : null;
    }

    #endregion
}