namespace StargateAPI.Business.Services
{
    public interface IApiLoggingService
    {
        Task LogRequestAsync(string requestType, object request, object response, int statusCode, long durationMs);
        Task LogExceptionAsync(string requestType, object? request, Exception exception, long durationMs, int? statusCode = null, object? response = null);
    }
}
