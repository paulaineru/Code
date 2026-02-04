using System.Text.Json.Serialization;

namespace AuthenticationService.Middleware
{
    public class RequestResponseLog
    {
        [JsonPropertyName("correlationId")]
        public string CorrelationId { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("request")]
        public RequestLog Request { get; set; } = new();

        [JsonPropertyName("response")]
        public ResponseLog Response { get; set; } = new();

        [JsonPropertyName("duration")]
        public long Duration { get; set; }
    }

    public class RequestLog
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("queryString")]
        public string QueryString { get; set; } = string.Empty;

        [JsonPropertyName("headers")]
        public Dictionary<string, string> Headers { get; set; } = new();

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; } = string.Empty;

        [JsonPropertyName("userAgent")]
        public string UserAgent { get; set; } = string.Empty;

        [JsonPropertyName("requestSize")]
        public long RequestSize { get; set; }
    }

    public class ResponseLog
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("headers")]
        public Dictionary<string, string> Headers { get; set; } = new();

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("responseSize")]
        public long ResponseSize { get; set; }
    }
} 