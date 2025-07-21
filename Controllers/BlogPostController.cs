using BlogMvc.Models.Common;
using BlogMvc.Models.ViewModels;
using BlogMvc.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlogMvc.Controllers;

[Authorize] // Require authentication by default for view controller
public class BlogPostController : Controller
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
    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.GetAllPostsWithPermissionsAsync(userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error?.Message ?? "Failed to load posts.";
            return View(new List<BlogPostViewModel>());
        }

        return View(result.Value);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _blogService.GetPostWithPermissionsAsync(id, userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error?.Message ?? "Post not found.";
            return RedirectToAction("Index");
        }

        return View(result.Value);
    }

    [HttpGet]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> MyPosts()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _blogService.GetPostsByUserIdAsync(userId);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error?.Message ?? "Failed to load your posts.";
            return View(new List<BlogPostViewModel>());
        }

        return View(result.Value);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> RecentPosts(int count = 10)
    {
        var result = await _blogService.GetRecentPostsAsync(count);

        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error?.Message ?? "Failed to load recent posts.";
            return RedirectToAction("Index");
        }

        return View(result.Value);
    }

    #endregion

    #region Command Operations

    [HttpGet]
    [Authorize(Roles = "User,Admin")]
    public IActionResult CreatePost()
    {
        return View(new CreatePostViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> CreatePost(CreatePostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _blogService.CreatePostAsync(model, userId);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Failed to create post.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Post successfully created!";
        return RedirectToAction("Details", new { id = result.Value!.Id });
    }

    [HttpGet]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _blogService.GetPostByIdAsync(id);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Error?.Message ?? "Post not found.";
            return RedirectToAction("Index");
        }

        var post = result.Value!;
        var editModel = new EditPostViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            Author = post.Author
        };

        return View(editModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> Edit(EditPostViewModel model)
    {
        // Add debugging
        Console.WriteLine($"Edit POST called - Model ID: {model.Id}");

        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is invalid");
            return View(model);
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _blogService.UpdatePostAsync(model, userId);

        if (!result.IsSuccess)
        {
            Console.WriteLine($"Service failed: {result.Error?.Message}");
            ModelState.AddModelError(string.Empty, result.Error?.Message ?? "Failed to update post.");
            return View(model);
        }

        Console.WriteLine("Post updated successfully");
        TempData["SuccessMessage"] = "Post successfully updated!";
        return RedirectToAction("Details", new { id = result.Value!.Id });
    }

    [HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "User,Admin")]
public async Task<IActionResult> Delete(int id)
{
    var userId = GetCurrentUserId();
    if (string.IsNullOrEmpty(userId))
    {
        return Json(new { success = false, message = "Unauthorized request." });
    }

    var result = await _blogService.DeletePostAsync(id, userId);

    if (!result.IsSuccess)
    {
        return Json(new { success = false, message = result.Error?.Message ?? "Failed to delete post." });
    }

    return Json(new { success = true, message = "Post successfully deleted!" });
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