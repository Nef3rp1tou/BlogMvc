using BlogMvc.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace BlogMvc.Controllers;

public abstract class BaseController : ControllerBase
{
    protected IActionResult CreateResponse<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return result.Error!.Code switch
        {
            "NOT_FOUND" => NotFound(CreateErrorResponse(result.Error)),
            "VALIDATION_ERROR" => BadRequest(CreateErrorResponse(result.Error)),
            "UNAUTHORIZED" => Unauthorized(CreateErrorResponse(result.Error)),
            "FORBIDDEN" => StatusCode(StatusCodes.Status403Forbidden, CreateErrorResponse(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, CreateErrorResponse(result.Error))
        };
    }

    protected IActionResult CreateResponse(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return result.Error!.Code switch
        {
            "NOT_FOUND" => NotFound(CreateErrorResponse(result.Error)),
            "VALIDATION_ERROR" => BadRequest(CreateErrorResponse(result.Error)),
            "UNAUTHORIZED" => Unauthorized(CreateErrorResponse(result.Error)),
            "FORBIDDEN" => StatusCode(StatusCodes.Status403Forbidden, CreateErrorResponse(result.Error)),
            _ => StatusCode(StatusCodes.Status500InternalServerError, CreateErrorResponse(result.Error))
        };
    }

    private static DummyError CreateErrorResponse(Error error)
    {
        return new DummyError
        {
            Code = error.Code,
            Message = error.Message,
            Metadata = error.Metadata
        };
    }
}

public class DummyError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object>? Metadata { get; set; }
}