using Bank.API.DTOs;
using System.Text.Json;

namespace Bank.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware.
    /// Returns clean JSON error responses and logs exceptions.
    /// Stack traces are never exposed to clients.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error: {Message}", ex.Message);
                await WriteErrorResponseAsync(context, ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business rule violation: {Message}", ex.Message);
                await WriteErrorResponseAsync(context, ex.Message, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await WriteErrorResponseAsync(context,
                    "An internal server error occurred. Please try again later.",
                    StatusCodes.Status500InternalServerError);
            }
        }

        private static Task WriteErrorResponseAsync(
            HttpContext context, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = statusCode;

            var response = new ResponseDto<object>
            {
                Success = false,
                Message = message,
                Data    = null
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
