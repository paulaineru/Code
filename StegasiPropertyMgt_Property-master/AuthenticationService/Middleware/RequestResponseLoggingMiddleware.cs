using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AuthenticationService.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            // Capture request details
            var requestLog = await CaptureRequestAsync(context);

            // Store the original response body stream
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);

                // Capture response details
                var responseLog = await CaptureResponseAsync(context, responseBodyStream);

                // Create log entry
                var logEntry = new RequestResponseLog
                {
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow,
                    Request = requestLog,
                    Response = responseLog,
                    Duration = stopwatch.ElapsedMilliseconds
                };

                // Log the request/response
                LogRequestResponse(logEntry);

                // Copy the response body back to the original stream
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalBodyStream);
            }
            finally
            {
                stopwatch.Stop();
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task<RequestLog> CaptureRequestAsync(HttpContext context)
        {
            var request = context.Request;
            var requestLog = new RequestLog
            {
                Method = request.Method,
                Path = request.Path,
                QueryString = request.QueryString.ToString(),
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            // Log request body for non-GET requests and if it's a JSON content type
            if (request.Method != "GET" && request.ContentType?.Contains("application/json") == true)
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true);
                requestLog.Body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            // Log additional request details
            requestLog.UserAgent = request.Headers["User-Agent"].ToString();
            requestLog.RequestSize = request.ContentLength ?? 0;

            return requestLog;
        }

        private async Task<ResponseLog> CaptureResponseAsync(HttpContext context, MemoryStream responseBodyStream)
        {
            var responseLog = new ResponseLog
            {
                StatusCode = context.Response.StatusCode,
                Headers = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            // Log response body if it's a JSON content type
            if (context.Response.ContentType?.Contains("application/json") == true)
            {
                try
                {
                    // Create a copy of the response body stream
                    using var memoryStream = new MemoryStream();
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // Read the response body from the copy
                    using var reader = new StreamReader(memoryStream);
                    responseLog.Body = await reader.ReadToEndAsync();

                    // Reset the original stream position
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error capturing response body");
                }
            }

            // Log additional response details
            responseLog.ResponseSize = responseBodyStream.Length;

            return responseLog;
        }

        private void LogRequestResponse(RequestResponseLog logEntry)
        {
            var logLevel = GetLogLevel(logEntry.Response.StatusCode);
            var logMessage = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
            {
                WriteIndented = _environment.IsDevelopment()
            });

            _logger.Log(logLevel, "HTTP {Method} {Path} responded {StatusCode} in {Duration}ms. CorrelationId: {CorrelationId}",
                logEntry.Request.Method,
                logEntry.Request.Path,
                logEntry.Response.StatusCode,
                logEntry.Duration,
                logEntry.CorrelationId);

            if (_environment.IsDevelopment())
            {
                _logger.LogDebug("Request/Response details: {Details}", logMessage);
            }
        }

        private static LogLevel GetLogLevel(int statusCode)
        {
            return statusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _ => LogLevel.Information
            };
        }
    }

    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
} 