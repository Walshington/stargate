using System.Net;
using System.Text.Json;
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
            UnprocessableEntityException => (HttpStatusCode.UnprocessableEntity, ErrorDetail.Create("UNPROCESSABLE_ENTITY", exception.Message, null)),
            BadHttpRequestException => (HttpStatusCode.BadRequest, ErrorDetail.Create("BAD_REQUEST", exception.Message, null)),
            _ => (HttpStatusCode.InternalServerError, ErrorDetail.Create("INTERNAL_ERROR", exception.Message, $"Exception type: {exception.GetType().Name}"))
        };

        /* Log the exception based on the status code */
        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Internal server error occurred: {Message}", exception.Message);
        else
            _logger.LogWarning("Client error ({StatusCode}): {Message}", (int)statusCode, exception.Message);

        /* Create the response */
        BaseResponse response = new BaseResponse
        {
            Success = false,
            Message = errorDetail.Message,
            ResponseCode = (int)statusCode,
            Error = errorDetail
        };

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
