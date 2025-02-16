using CurrencyConverter.ApplicationServices;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Net;

namespace CurrencyConverter.Infrastructure
{
    public class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> _logger) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error");
                await HandleException(context, e);
            }
        }

        public static Task HandleException(HttpContext context, Exception exception)
        {
            return exception switch
            {
                ValidationException e => WriteResponse(context, HttpStatusCode.BadRequest, new { }),
                EntityNotFoundException e => WriteResponse(context, HttpStatusCode.NotFound, new { }),
                BrokenCircuitException e => WriteResponse(context, HttpStatusCode.ServiceUnavailable, new { }),
                _ => WriteResponse(context, HttpStatusCode.InternalServerError, new { })
            };
        }

        public async static Task WriteResponse(HttpContext context, HttpStatusCode httpStatusCode, object value)
        {
            context.Response.StatusCode = (int)httpStatusCode;
            await context.Response.WriteAsJsonAsync(value, SerializerSettings.Default);
        }
    }
}
