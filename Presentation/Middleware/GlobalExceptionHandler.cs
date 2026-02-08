using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BusinessLogic.Exceptions;
using BusinessLogic.Wrappers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Presentation.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception has occurred.");

            var (statusCode, message) = MapException(exception);

            // Không expose raw message nếu là lỗi 500
            var safeMessage = exception is CustomException
                ? message
                : "An unexpected error occurred.";

            var response = new ApiResponse<object>
            {
                Succeeded = false,
                Message = safeMessage,
                StatusCode = statusCode,
                Data = null
            };

            // Thêm traceId để debug production
            httpContext.Response.Headers["X-Trace-Id"] = httpContext.TraceIdentifier;

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }

        private static (int StatusCode, string Message) MapException(Exception exception) => exception switch
        {
            //NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            //BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
            //ValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
            //UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            //ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            //CustomException customEx => (customEx.StatusCode, customEx.Message), // Fallback for other custom exceptions
            //_ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            BadRequestException ex => (StatusCodes.Status400BadRequest, ex.Message),
            ValidationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            UnauthorizedException ex => (StatusCodes.Status401Unauthorized, ex.Message),
            ForbiddenException ex => (StatusCodes.Status403Forbidden, ex.Message),
            CustomException ex => (ex.StatusCode, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };
    }
}
