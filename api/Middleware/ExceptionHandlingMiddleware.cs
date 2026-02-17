using System.Net;
using System.Text.Json;
using StargateAPI.Business.Services;
using StargateAPI.Domain;
using StargateAPI.Domain.Exceptions;

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

    /* Handle the exception and return the appropriate response */
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        /* Get the status code and error detail based on the exception */
        var (statusCode, errorDetail) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, ErrorDetail.Create("NOT_FOUND", exception.Message, null)),
            BadRequestException => (HttpStatusCode.BadRequest, ErrorDetail.Create("BAD_REQUEST", exception.Message, null)),
            ConflictException => (HttpStatusCode.Conflict, ErrorDetail.Create("CONFLICT", exception.Message, null)),
            UnprocessableEntityException => (HttpStatusCode.UnprocessableEntity, ErrorDetail.Create("UNPROCESSABLE_ENTITY", exception.Message, null)),
            BadHttpRequestException => (HttpStatusCode.BadRequest, ErrorDetail.Create("BAD_REQUEST", exception.Message, null)),
            _ => (HttpStatusCode.InternalServerError, ErrorDetail.Create("INTERNAL_ERROR", exception.Message, $"Exception type: {exception.GetType().Name}"))
        };

        /* Log exception based on severity */
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Internal server error occurred: {Message}", exception.Message);
        else
            _logger.LogWarning("Client error ({StatusCode}): {Message}", (int)statusCode, exception.Message);

        /* Build the error response that will be sent to the client */
        BaseResponse response = new BaseResponse
        {
            Success = false,
            ResponseCode = (int)statusCode,
            Error = errorDetail
        };

        /*
         * Log exception to database with HTTP context and error response.
         * Request data contains HTTP context (method, path, query).
         * Response data contains the actual error response sent to the client.
         */
        try
        {
            var loggingService = context.RequestServices.GetRequiredService<IApiLoggingService>();

            // Build HTTP context information for request data
            var httpContext = new
            {
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.Value
            };

            await loggingService.LogExceptionAsync("HTTP_REQUEST", httpContext, exception, 0, (int)statusCode, response);
        }
        catch (Exception logEx)
        {
            /* Fail-safe: Don't let logging failures break exception handling */
            _logger.LogError(logEx, "Failed to log exception to database");
        }

        /* Set the response content type and status code */
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        /* Serialize the response to JSON */
        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
