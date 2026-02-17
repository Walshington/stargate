using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Services
{
    public class ApiLoggingService : IApiLoggingService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApiLoggingService> _logger;

        /*
         * IServiceProvider is injected instead of StargateContext directly because we need to create
         * separate scopes with isolated DbContext instances for each log operation.
         * This ensures transaction isolation between logging and business operations.
         */
        public ApiLoggingService(IServiceProvider serviceProvider, ILogger<ApiLoggingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task LogRequestAsync(string requestType, object request, object response, int statusCode, long durationMs)
        {
            /* Log successful request to console for debugging */
            _logger.LogInformation("Request: {RequestType} completed in {Duration}ms with status {StatusCode}",
                requestType, durationMs, statusCode);

            try
            {
                /*
                 * Create a separate scope to get an isolated DbContext instance.
                 * This ensures logging has its own database transaction, independent of the business operation.
                 * If the business transaction rolls back, the log entry will still persist.
                 */
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<StargateContext>();

                /* Serialize request and response objects to formatted JSON with camelCase (matches API responses) */
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var requestJson = JsonSerializer.Serialize(request, jsonOptions);
                var responseJson = JsonSerializer.Serialize(response, jsonOptions);

                /* Create log entry with request/response data and execution metrics */
                var apiLog = new ApiLog
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Information",
                    RequestType = requestType,
                    RequestData = requestJson,
                    ResponseData = responseJson,
                    StatusCode = statusCode,
                    DurationMs = durationMs
                };

                await context.ApiLogs.AddAsync(apiLog);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                /* Fail-safe: Don't let logging failures break the API */
                _logger.LogError(ex, "Failed to log request to database: {RequestType}", requestType);
            }
        }

        public async Task LogExceptionAsync(string requestType, object? request, Exception exception, long durationMs, int? statusCode = null, object? response = null)
        {
            /* Log exception to console for debugging */
            _logger.LogError(exception, "Request: {RequestType} failed after {Duration}ms with status {StatusCode} - {ExceptionMessage}",
                requestType, durationMs, statusCode, exception.Message);

            try
            {
                /*
                 * Create a separate scope to get an isolated DbContext instance.
                 * This ensures exception logs are saved even if the main operation fails and rolls back.
                 */
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<StargateContext>();

                /* Serialize request and response objects to formatted JSON with camelCase (matches API responses) */
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var requestJson = request != null
                    ? JsonSerializer.Serialize(request, jsonOptions)
                    : "null";

                var responseJson = response != null
                    ? JsonSerializer.Serialize(response, jsonOptions)
                    : null;

                /* Create log entry with exception details, request context, and error response */
                var apiLog = new ApiLog
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "Error",
                    RequestType = requestType,
                    RequestData = requestJson,
                    ResponseData = responseJson,
                    ExceptionMessage = exception.Message,
                    ExceptionStackTrace = exception.StackTrace,
                    StatusCode = statusCode,
                    DurationMs = durationMs
                };

                await context.ApiLogs.AddAsync(apiLog);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                /* Fail-safe: Don't let logging failures break the API */
                _logger.LogError(ex, "Failed to log exception to database: {RequestType}", requestType);
            }
        }
    }
}
