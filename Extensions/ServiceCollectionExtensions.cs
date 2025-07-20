using BlogMvc.Repositories;
using BlogMvc.Repositories.Interfaces;
using BlogMvc.Services;
using BlogMvc.Services.Interfaces;
using BlogMvc.Validators;
using FluentValidation;

namespace BlogMvc.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {

        // Repositories
        services.AddScoped<IBlogRepository, BlogRepository>();

        // Services
        services.AddScoped<IBlogService, BlogService>();

        // Validators
        services.AddScoped<BlogPostValidator>();
        services.AddScoped<CreatePostViewModelValidator>();
        services.AddScoped<EditPostViewModelValidator>();
        services.AddScoped<SearchViewModelValidator>();

        // Register all validators from assembly (alternative approach)
        // services.AddValidatorsFromAssemblyContaining<CreatePostViewModelValidator>();

        return services;
    }
}