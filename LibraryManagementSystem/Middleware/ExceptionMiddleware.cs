using System.Net;
using System.Text.Json;
using FluentValidation;
using LibraryManagementSystem.DTOs.Common;

namespace LibraryManagementSystem.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message, errors) = exception switch
            {
                KeyNotFoundException ex    => (HttpStatusCode.NotFound,             ex.Message,    (IEnumerable<string>?)null),
                InvalidOperationException ex => (HttpStatusCode.BadRequest,          ex.Message,    (IEnumerable<string>?)null),
                ArgumentException ex       => (HttpStatusCode.BadRequest,            ex.Message,    (IEnumerable<string>?)null),
                ValidationException ex     => (HttpStatusCode.UnprocessableEntity,   "Validation failed.", ex.Errors.Select(e => e.ErrorMessage)),
                _                          => (HttpStatusCode.InternalServerError,   "An unexpected error occurred.", (IEnumerable<string>?)null)
            };

            context.Response.StatusCode = (int)statusCode;

            var response = ApiResponse<object>.FailResult(message, errors);

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
