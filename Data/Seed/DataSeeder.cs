using BlogMvc.Data;
using BlogMvc.Models;
using BlogMvc.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogMvc.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created (for in-memory database)
        await context.Database.EnsureCreatedAsync();

        // Seed Roles
        await SeedRolesAsync(roleManager);

        // Seed Users
        await SeedUsersAsync(userManager);

        // Seed Blog Posts
        await SeedBlogPostsAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in UserRoles.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager)
    {
        // Seed Admin User
        var adminEmail = "admin@blogmvc.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(adminUser, "Admin123!");
            await userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
        }

        // Seed Regular Users
        var regularUsers = new[]
        {
            new { Email = "john.doe@example.com", Name = "John Doe", Password = "User123!" },
            new { Email = "jane.smith@example.com", Name = "Jane Smith", Password = "User123!" },
            new { Email = "bob.wilson@example.com", Name = "Bob Wilson", Password = "User123!" }
        };

        foreach (var userData in regularUsers)
        {
            var user = await userManager.FindByEmailAsync(userData.Email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = userData.Email,
                    Email = userData.Email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, userData.Password);
                await userManager.AddToRoleAsync(user, UserRoles.User);
            }
        }

        // Note: Guest role is for non-authenticated users, so we don't create guest accounts
    }

    private static async Task SeedBlogPostsAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        // Check if we already have blog posts
        if (await context.BlogPosts.AnyAsync())
        {
            return;
        }

        // Get seeded users
        var adminUser = await userManager.FindByEmailAsync("admin@blogmvc.com");
        var johnUser = await userManager.FindByEmailAsync("john.doe@example.com");
        var janeUser = await userManager.FindByEmailAsync("jane.smith@example.com");
        var bobUser = await userManager.FindByEmailAsync("bob.wilson@example.com");

        var blogPosts = new List<BlogPost>
        {
            // Admin posts
            new BlogPost
            {
                Title = "Welcome to Our New Blog Platform",
                Content = "We're excited to announce the launch of our new blog platform built with ASP.NET Core MVC! " +
                         "This platform features a clean architecture with the Result pattern, role-based authorization, " +
                         "and a modern, responsive design. Stay tuned for more updates and features!",
                Author = "Admin",
                UserId = adminUser!.Id,
                PublishedDate = DateTime.Now.AddDays(-30)
            },
            new BlogPost
            {
                Title = "Platform Guidelines and Best Practices",
                Content = "As we grow our community, we want to ensure everyone has a great experience. " +
                         "Please follow these guidelines: Be respectful, share quality content, cite your sources, " +
                         "and engage constructively with other users. Happy blogging!",
                Author = "Admin",
                UserId = adminUser.Id,
                PublishedDate = DateTime.Now.AddDays(-28)
            },

            // John's posts
            new BlogPost
            {
                Title = "Getting Started with Clean Architecture in .NET",
                Content = "Clean Architecture is a software design philosophy that separates the elements of a design " +
                         "into ring levels. The main rule of clean architecture is that code dependencies can only " +
                         "move from the outer levels inward. This approach makes your application more maintainable, " +
                         "testable, and independent of frameworks, databases, and external agencies.",
                Author = "John Doe",
                UserId = johnUser!.Id,
                PublishedDate = DateTime.Now.AddDays(-20)
            },
            new BlogPost
            {
                Title = "Understanding the Result Pattern",
                Content = "The Result pattern is a way to handle errors without throwing exceptions. Instead of using " +
                         "try-catch blocks everywhere, we return a Result object that contains either a success value " +
                         "or an error. This makes error handling explicit, predictable, and easier to test. " +
                         "It's particularly useful in functional programming approaches.",
                Author = "John Doe",
                UserId = johnUser.Id,
                PublishedDate = DateTime.Now.AddDays(-15)
            },
            new BlogPost
            {
                Title = "Repository Pattern Best Practices",
                Content = "The Repository pattern is a crucial part of Domain-Driven Design. It provides an abstraction " +
                         "of data access, allowing you to centralize query logic and make your application more testable. " +
                         "Keep your repositories focused on data access only - business logic belongs in the service layer!",
                Author = "John Doe",
                UserId = johnUser.Id,
                PublishedDate = DateTime.Now.AddDays(-10)
            },

            // Jane's posts
            new BlogPost
            {
                Title = "Building Modern Web APIs with ASP.NET Core",
                Content = "ASP.NET Core provides a powerful framework for building Web APIs. With features like " +
                         "dependency injection, middleware pipeline, and attribute routing, you can build scalable " +
                         "and maintainable APIs. Don't forget to implement proper versioning, documentation with " +
                         "Swagger, and consistent error handling!",
                Author = "Jane Smith",
                UserId = janeUser!.Id,
                PublishedDate = DateTime.Now.AddDays(-18)
            },
            new BlogPost
            {
                Title = "Role-Based Authorization in Practice",
                Content = "Implementing role-based authorization is crucial for securing your application. " +
                         "ASP.NET Core Identity provides a robust foundation with built-in support for roles, " +
                         "claims, and policies. Remember to follow the principle of least privilege and regularly " +
                         "audit your authorization logic.",
                Author = "Jane Smith",
                UserId = janeUser.Id,
                PublishedDate = DateTime.Now.AddDays(-12)
            },
            new BlogPost
            {
                Title = "FluentValidation: Making Validation Elegant",
                Content = "FluentValidation is a .NET library for building strongly-typed validation rules. " +
                         "It provides a fluent interface for defining validation logic, making your code more " +
                         "readable and maintainable. You can easily create complex validation scenarios and " +
                         "keep your validation logic separate from your business logic.",
                Author = "Jane Smith",
                UserId = janeUser.Id,
                PublishedDate = DateTime.Now.AddDays(-5)
            },

            // Bob's posts
            new BlogPost
            {
                Title = "In-Memory Database Testing Strategies",
                Content = "In-memory databases are perfect for testing scenarios. They're fast, don't require " +
                         "external dependencies, and can be easily reset between tests. When using Entity Framework Core, " +
                         "the in-memory provider is great for unit tests, but remember it doesn't enforce all constraints " +
                         "like a real database would.",
                Author = "Bob Wilson",
                UserId = bobUser!.Id,
                PublishedDate = DateTime.Now.AddDays(-8)
            },
            new BlogPost
            {
                Title = "Dependency Injection Patterns and Anti-Patterns",
                Content = "Dependency Injection is a fundamental concept in modern software development. " +
                         "It promotes loose coupling and makes your code more testable. However, be aware of " +
                         "anti-patterns like Service Locator, excessive interface segregation, and circular dependencies. " +
                         "Keep your dependency graphs simple and your constructors lean.",
                Author = "Bob Wilson",
                UserId = bobUser.Id,
                PublishedDate = DateTime.Now.AddDays(-3)
            },
            new BlogPost
            {
                Title = "Performance Optimization Tips for .NET Applications",
                Content = "Performance optimization should be based on measurements, not assumptions. Use profiling tools, " +
                         "understand async/await properly, minimize allocations, and cache wisely. Remember that " +
                         "premature optimization is the root of all evil - first make it work, then make it right, " +
                         "and finally make it fast if needed.",
                Author = "Bob Wilson",
                UserId = bobUser.Id,
                PublishedDate = DateTime.Now.AddDays(-1)
            }
        };

        await context.BlogPosts.AddRangeAsync(blogPosts);
        await context.SaveChangesAsync();
    }
}