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
                ret = new UnprocessableEntityObjectResult(new { Error = result.ErrorMessage, Data = data });
                break;
            case ResponseStatus.NotFound:
                ret = new NotFoundObjectResult(new { Error = result.ErrorMessage, Data = data });
                break;
            case ResponseStatus.InternalError:
                ret = new ObjectResult(new { Error = result.ErrorMessage, Data = data }) { StatusCode = (int)HttpStatusCode.InternalServerError };
                break;
            default:
                ret = new StatusCodeResult((int)HttpStatusCode.InternalServerError);
                break;
        }

        return ret;
    }
}