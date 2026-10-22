using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using NWSDB.API.DTOs;
using NWSDB.API.Exceptions;

namespace NWSDB.API.Middleware
{
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
            catch (ArgumentException ex)
            {
                // Business logic validation error
                _logger.LogWarning(ex, "Validation error occurred.");
                await HandleExceptionAsync(context, ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (BankApiException ex)
            {
                // Downstream Bank.API error or business rejection
                var statusCode = (int)ex.StatusCode;
                _logger.LogWarning(ex, "Bank.API response exception | Status: {StatusCode} | Message: {Message}", statusCode, ex.Message);
                await HandleExceptionAsync(context, ex.Message, statusCode);
            }
            catch (Exception ex)
            {
                // Unhandled system error
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, "An internal server error occurred.", StatusCodes.Status500InternalServerError);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, string message, int statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ResponseDto<object>
            {
                Success = false,
                Message = message,
                Data = null
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
