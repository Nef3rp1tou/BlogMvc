using System.Diagnostics;
using BlogMvc.Models;
using BlogMvc.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace BlogMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            IBlogService blogService,
            UserManager<IdentityUser> userManager)
        {
            _blogService = blogService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            const int pageSize = 6; // Number of posts per page

            var userId = User.Identity?.IsAuthenticated == true
                ? _userManager.GetUserId(User)
                : null;

            var result = !string.IsNullOrWhiteSpace(searchTerm)
                ? await _blogService.SearchPostsByTitleAsync(searchTerm)
                : await _blogService.GetAllPostsWithPermissionsAsync(userId);

            if (!result.IsSuccess)
            {
                ViewBag.ErrorMessage = result.Error?.Message ?? "An error occurred while loading posts.";
                return View(new List<BlogMvc.Models.ViewModels.BlogPostViewModel>());
            }

            var posts = result.Value?.ToList() ?? new List<BlogMvc.Models.ViewModels.BlogPostViewModel>();

            // Calculate pagination
            var totalPosts = posts.Count;
            var totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);
            var pagedPosts = posts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Pass pagination data to view
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            return View(pagedPosts);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}