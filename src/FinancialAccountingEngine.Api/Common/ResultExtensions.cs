using FinancialAccountingEngine.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FinancialAccountingEngine.Api.Common;

/// <summary>Maps the application <see cref="Result"/> type onto HTTP responses with ProblemDetails.</summary>
public static class ResultExtensions
{
    public static IActionResult ToOkResult<TValue>(this ControllerBase controller, Result<TValue> result)
        => result.IsSuccess
            ? controller.Ok(result.Value)
            : controller.ToProblem(result.Error);

    public static IActionResult ToProblem(this ControllerBase controller, Error error)
    {
        var statusCode = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "conflict" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return controller.Problem(
            detail: error.Message,
            statusCode: statusCode,
            title: error.Code switch
            {
                "not_found" => "Resource not found",
                "conflict" => "Conflict",
                _ => "Validation failed"
            });
    }
}
