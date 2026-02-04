using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System.Text.Json;

namespace AuthenticationService.Middleware
{
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private readonly IElasticClient _elasticClient;

        public PerformanceMonitoringMiddleware(
            RequestDelegate next,
            ILogger<PerformanceMonitoringMiddleware> logger,
            IElasticClient elasticClient)
        {
            _next = next;
            _logger = logger;
            _elasticClient = elasticClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var metrics = new
                {
                    Timestamp = startTime,
                    Duration = stopwatch.ElapsedMilliseconds,
                    Path = context.Request.Path,
                    Method = context.Request.Method,
                    StatusCode = context.Response.StatusCode,
                    CorrelationId = context.Items["CorrelationId"]?.ToString()
                };

                _logger.LogInformation(
                    "Request {Method} {Path} completed in {Duration}ms with status {StatusCode}",
                    metrics.Method,
                    metrics.Path,
                    metrics.Duration,
                    metrics.StatusCode);

                // Index the metrics in Elasticsearch
                await _elasticClient.IndexDocumentAsync(metrics);
            }
        }
    }

    public class PerformanceMonitoringOptions
    {
        public long SlowRequestThresholdMs { get; set; } = 1000; // 1 second
        public long HighMemoryThresholdBytes { get; set; } = 100 * 1024 * 1024; // 100 MB
    }

    public static class PerformanceMonitoringMiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder app)
        {
            return app.UseMiddleware<PerformanceMonitoringMiddleware>();
        }

        public static IServiceCollection AddPerformanceMonitoring(this IServiceCollection services, Action<PerformanceMonitoringOptions> configureOptions)
        {
            services.Configure(configureOptions);
            return services;
        }
    }
} 