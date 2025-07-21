using BlogMvc.Middleware;

namespace BlogMvc.Extensions;

public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds simple request logging middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseSimpleRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SimpleRequestLoggingMiddleware>();
    }
}