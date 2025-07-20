namespace BlogMvc.Models.Common;
public class Error(string code, string message, Dictionary<string, object>? metadata = null)
{
    public string Code { get; } = code;
    public string Message { get; } = message;
    public Dictionary<string, object>? Metadata { get; } = metadata;

    public static Error NotFound(string resource, object id) =>
                new("NOT_FOUND", $"{resource} with ID {id} was not found.");
    public static Error Validation(string message) =>
                new("VALIDATION_ERROR", message);
    public static Error Unauthorized(string message) =>
                new("UNAUTHORIZED", message);
    public static Error Forbidden(string message) =>
                new("FORBIDDEN", message);
    public static Error InternalServerError(string message) =>
                new("INTERNAL_SERVER_ERROR", message);
}

