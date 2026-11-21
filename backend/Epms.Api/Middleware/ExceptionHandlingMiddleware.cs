using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Epms.Api.Exceptions;

namespace Epms.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted && !context.RequestAborted.IsCancellationRequested)
        {
            var (status, detail) = exception switch
            {
                ApiException expected => (expected.StatusCode, expected.Message),
                DbUpdateConcurrencyException => (409, "The record changed. Refresh and retry."),
                DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } }
                    or DbUpdateException { InnerException: SqliteException { SqliteExtendedErrorCode: 1555 or 2067 } }
                    => (409, "A record with the same unique value already exists."),
                _ => (500, "An unexpected error occurred.")
            };
            if (status == 500)
                logger.LogError(exception, "Unhandled error in request {TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = Microsoft.AspNetCore.WebUtilities.ReasonPhrases.GetReasonPhrase(status),
                Detail = detail,
                Instance = context.Request.Path
            }, options: null, contentType: "application/problem+json");
        }
    }
}
