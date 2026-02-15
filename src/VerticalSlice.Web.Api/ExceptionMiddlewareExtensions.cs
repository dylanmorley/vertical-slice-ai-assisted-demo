using System.Diagnostics.CodeAnalysis;
using System.Net;
using CQRS.Mediatr.Lite.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace VerticalSlice.Web.Api;

[ExcludeFromCodeCoverage]
public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app) =>
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                // Safely attempt to write a response
                await SafeWriteErrorResponse(context, () =>
                {
                    IExceptionHandlerFeature? contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        context.Response.StatusCode = contextFeature.Error switch
                        {
                            KeyNotFoundException => (int)HttpStatusCode.NotFound,
                            InvalidOperationException ex when ex.Message.Contains("not found",
                                StringComparison.OrdinalIgnoreCase) => (int)HttpStatusCode.NotFound,
                            InvalidOperationException or RequestValidationException => (int)HttpStatusCode.BadRequest,
                            _ => (int)HttpStatusCode.InternalServerError
                        };

                        return new ProblemDetails
                        {
                            Status = context.Response.StatusCode,
                            Title = "An unexpected error occurred",
                            Detail = contextFeature.Error.Message,
                            Instance = context.Request.Path
                        };
                    }

                    return null;
                });
            });
        });

    private static async Task SafeWriteErrorResponse(HttpContext context, Func<ProblemDetails?> getProblemDetails)
    {
        ILogger? logger = context.RequestServices.GetService<ILogger>();

        try
        {
            ProblemDetails? problem = getProblemDetails();
            if (problem != null)
            {
                await WriteJsonResponse(context, problem);
                return;
            }
        }
        catch (Exception ex)
        {
            // Log the exception safely
            logger?.LogError(ex, "Error in exception handler");
        }

        // Fallback response
        try
        {
            ProblemDetails fallbackProblem = new()
            {
                Status = 500,
                Title = "An unexpected error occurred",
                Detail = "An unexpected error occurred",
                Instance = context.Request.Path
            };
            await WriteJsonResponse(context, fallbackProblem);
        }
        catch (Exception ex)
        {
            // Last resort - try to write a minimal response
            logger?.LogError(ex, "Critical error in exception handler fallback");
            await WriteMinimalErrorResponse(context);
        }
    }

    private static async Task WriteJsonResponse(HttpContext context, ProblemDetails problem)
    {
        // Ensure we haven't already started writing the response
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = problem.Status ?? 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static async Task WriteMinimalErrorResponse(HttpContext context)
    {
        try
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Internal Server Error");
            }
        }
        catch
        {
            // If even this fails, we can't do anything more
            // The connection will be terminated
        }
    }
}
