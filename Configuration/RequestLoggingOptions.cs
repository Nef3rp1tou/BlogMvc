namespace BlogMvc.Configuration;

public class RequestLoggingOptions
{
    public bool LogRequestBody { get; set; } = true;
    public bool LogResponseBody { get; set; } = true;
    public bool LogHeaders { get; set; } = true;
    public int MaxBodyLength { get; set; } = 1000;
    public long SlowRequestThresholdMs { get; set; } = 5000;
    public List<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/swagger",
        "/favicon.ico"
    };
    public List<string> SensitiveHeaders { get; set; } = new()
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-API-Key",
        "Authentication"
    };
}

// In Program.cs:
// builder.Services.Configure<RequestLoggingOptions>(
//     builder.Configuration.GetSection("RequestLogging"));

// In appsettings.json:
// {
//   "RequestLogging": {
//     "LogRequestBody": true,
//     "LogResponseBody": false,
//     "MaxBodyLength": 500,
//     "SlowRequestThresholdMs": 3000,
//     "ExcludedPaths": ["/health", "/metrics"]
//   }
// }