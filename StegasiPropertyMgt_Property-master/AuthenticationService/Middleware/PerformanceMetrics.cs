using System.Text.Json.Serialization;

namespace AuthenticationService.Middleware
{
    public class PerformanceMetrics
    {
        [JsonPropertyName("endpoint")]
        public string Endpoint { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("duration")]
        public long DurationMs { get; set; }

        [JsonPropertyName("memoryUsage")]
        public long MemoryUsageBytes { get; set; }

        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("isSuccess")]
        public bool IsSuccess { get; set; }

        [JsonPropertyName("errorCount")]
        public int ErrorCount { get; set; }

        [JsonPropertyName("requestSize")]
        public long RequestSizeBytes { get; set; }

        [JsonPropertyName("responseSize")]
        public long ResponseSizeBytes { get; set; }
    }
} 