using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Greenix.Shared.Models;

namespace Greenix.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred during request execution. CorrelationId: {TraceId}", context.TraceIdentifier);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // In production, we don't leak raw database/stack errors. We return a generic message.
            var message = _env.IsDevelopment() 
                ? exception.Message 
                : "An unexpected error occurred on the server. Please contact support.";

            var details = _env.IsDevelopment() && exception.StackTrace != null 
                ? exception.StackTrace 
                : $"Trace Identifier: {context.TraceIdentifier}";

            var errorsList = new List<string> { message };
            if (_env.IsDevelopment())
            {
                errorsList.Add(details);
            }

            var response = new BaseResponse<object>(errorsList, "Internal Server Error");
            
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}
