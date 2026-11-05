using System.Net;
using Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ResponseResultExtensions
{
    public static ActionResult ToActionResult<T>(this ResponseResult<T> result)
    {
        return CreateObjectResult(result, result.Data);
    }

    public static ActionResult ToActionResult(this ResponseResult result)
    {
        return CreateObjectResult(result, null);
    }

    private static ActionResult CreateObjectResult(this ResponseResult result, object? data = null)
    {
        ActionResult ret;
        switch (result.Status)
        {
            case ResponseStatus.Success when data == null:
                ret = new OkResult();
                break;
            case ResponseStatus.Success:
                ret = new OkObjectResult(data);
                break;
            case ResponseStatus.InvalidOperation:
                ret = new UnprocessableEntityObjectResult(new ProblemDetails
                {
                    Status = (int)HttpStatusCode.UnprocessableEntity,
                    Title = "Unprocessable Content",
                    Detail = result.ErrorMessage
                });
                break;
            case ResponseStatus.NotFound:
                ret = new NotFoundObjectResult(new ProblemDetails
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Title = "Not Found",
                    Detail = result.ErrorMessage
                });
                break;
            case ResponseStatus.InternalError:
            default:
                ret = new ObjectResult(new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error"
                });
                break;
        }

        return ret;
    }
}