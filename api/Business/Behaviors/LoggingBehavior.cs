using System.Diagnostics;
using MediatR;
using StargateAPI.Business.Services;
using StargateAPI.Domain;
using StargateAPI.Domain.Exceptions;

namespace StargateAPI.Business.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IApiLoggingService _loggingService;

        public LoggingBehavior(IApiLoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestType = typeof(TRequest).Name;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                /* Execute the actual MediatR handler and measure execution time */
                var response = await next();
                stopwatch.Stop();

                /* Extract status code from response if available (defaults to 200) */
                var statusCode = 200;
                if (response is BaseResponse baseResponse)
                {
                    statusCode = baseResponse.ResponseCode;
                }

                /* Log successful request with response data */
                await _loggingService.LogRequestAsync(requestType, request, response!, statusCode, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                /*
                 * Log exception details with status code if available, then re-throw.
                 * Try to extract status code from exception if it's a domain exception with known status.
                 */
                stopwatch.Stop();
                
                // Try to infer status code from exception type (matches middleware logic)
                int? statusCode = ex switch
                {
                    NotFoundException => 404,
                    BadRequestException => 400,
                    ConflictException => 409,
                    UnprocessableEntityException => 422,
                    _ => 500
                };
                
                await _loggingService.LogExceptionAsync(requestType, request, ex, stopwatch.ElapsedMilliseconds, statusCode);
                throw;
            }
        }
    }
}
