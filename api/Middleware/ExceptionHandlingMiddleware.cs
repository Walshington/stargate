using StargateAPI.Domain;
using StargateAPI.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace StargateAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, details) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", null as string),
            BadRequestException => (HttpStatusCode.BadRequest, "BAD_REQUEST", null as string),
            BadHttpRequestException => (HttpStatusCode.BadRequest, "BAD_REQUEST", null as string),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", $"Exception type: {exception.GetType().Name}")
        };

        // Log based on severity
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Internal server error occurred: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Client error ({StatusCode}): {Message}", (int)statusCode, exception.Message);
        }

        var response = new BaseResponse
        {
            Success = false,
            Message = exception.Message,
            ResponseCode = (int)statusCode,
            Error = new ErrorDetail
            {
                Code = errorCode,
                Message = exception.Message,
                Details = details,
                Timestamp = DateTime.UtcNow
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
