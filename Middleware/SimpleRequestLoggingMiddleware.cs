using System.Diagnostics;
using System.Text;

namespace BlogMvc.Middleware;

public class SimpleRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SimpleRequestLoggingMiddleware> _logger;

    public SimpleRequestLoggingMiddleware(RequestDelegate next, ILogger<SimpleRequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for static files
        if (IsStaticFile(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;
        var isApiRequest = IsApiRequest(request.Path);

        // Log incoming request
        _logger.LogInformation("🔵 {Method} {Path} from {IP}",
            request.Method,
            request.Path,
            GetClientIP(context));

        // Log request body for API POST/PUT requests
        string? requestBody = null;
        if (isApiRequest && ShouldLogRequestBody(request))
        {
            requestBody = await ReadRequestBodyAsync(request);
            if (!string.IsNullOrEmpty(requestBody))
            {
                var truncatedBody = requestBody.Length > 500 ? requestBody[..500] + "..." : requestBody;
                _logger.LogInformation("📝 Request Body: {RequestBody}", truncatedBody);
            }
        }

        // Capture response for API requests
        Stream originalResponseBody = context.Response.Body;
        string? responseBody = null;

        try
        {
            if (isApiRequest)
            {
                using var responseMemoryStream = new MemoryStream();
                context.Response.Body = responseMemoryStream;

                await _next(context);

                // Read response body
                responseBody = await ReadResponseBodyAsync(responseMemoryStream);

                // Copy response back to original stream
                responseMemoryStream.Seek(0, SeekOrigin.Begin);
                await responseMemoryStream.CopyToAsync(originalResponseBody);
            }
            else
            {
                await _next(context);
            }
        }
        finally
        {
            stopwatch.Stop();

            // Log response
            _logger.LogInformation("🟢 {Method} {Path} -> {StatusCode} ({ElapsedMs}ms)",
                request.Method,
                request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            // Log response body for API requests
            if (isApiRequest && !string.IsNullOrEmpty(responseBody))
            {
                var truncatedResponse = responseBody.Length > 500 ? responseBody[..500] + "..." : responseBody;
                _logger.LogInformation("📤 Response Body: {ResponseBody}", truncatedResponse);
            }

            // Log slow requests
            if (stopwatch.ElapsedMilliseconds > 2000)
            {
                _logger.LogWarning("🐌 Slow request: {Method} {Path} took {ElapsedMs}ms",
                    request.Method,
                    request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }

    private static bool IsStaticFile(string path)
    {
        var staticExtensions = new[] { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".svg", ".woff", ".woff2", ".map" };
        return staticExtensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsApiRequest(string path)
    {
        return path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        if (request.Method != "POST" && request.Method != "PUT" && request.Method != "PATCH")
            return false;

        var contentType = request.ContentType?.ToLower();

        // Don't log file uploads or binary content
        if (contentType?.Contains("multipart/form-data") == true ||
            contentType?.Contains("application/octet-stream") == true)
        {
            return false;
        }

        // Log JSON content
        return contentType?.Contains("application/json") == true;
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            request.EnableBuffering();

            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0; // Reset position for next middleware

            return body;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static async Task<string> ReadResponseBodyAsync(MemoryStream responseStream)
    {
        try
        {
            responseStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseStream, Encoding.UTF8, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static string GetClientIP(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}