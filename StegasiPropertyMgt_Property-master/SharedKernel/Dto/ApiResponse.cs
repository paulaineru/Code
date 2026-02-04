using System.Text.Json.Serialization;

namespace SharedKernel.Dto
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        public ApiResponse(string code, string message, T? data = default)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public static ApiResponse<T> Success(T data, string message = "Success")
        {
            return new ApiResponse<T>("200", message, data);
        }

        public static ApiResponse<T> Error(string code, string message)
        {
            return new ApiResponse<T>(code, message, default);
        }

        public static ApiResponse<T> BadRequest(string message)
        {
            return new ApiResponse<T>("400", message, default);
        }

        public static ApiResponse<T> NotFound(string message)
        {
            return new ApiResponse<T>("404", message, default);
        }

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized")
        {
            return new ApiResponse<T>("401", message, default);
        }

        public static ApiResponse<T> Forbidden(string message = "Forbidden")
        {
            return new ApiResponse<T>("403", message, default);
        }

        public static ApiResponse<T> InternalServerError(string message = "Internal Server Error")
        {
            return new ApiResponse<T>("500", message, default);
        }
    }

    // Non-generic version for responses without data
    public class ApiResponse
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        public ApiResponse(string code, string message, object? data = null)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        public static ApiResponse Success(string message = "Success")
        {
            return new ApiResponse("200", message, null);
        }

        public static ApiResponse Error(string code, string message)
        {
            return new ApiResponse(code, message, null);
        }

        public static ApiResponse BadRequest(string message)
        {
            return new ApiResponse("400", message, null);
        }

        public static ApiResponse NotFound(string message)
        {
            return new ApiResponse("404", message, null);
        }

        public static ApiResponse Unauthorized(string message = "Unauthorized")
        {
            return new ApiResponse("401", message, null);
        }

        public static ApiResponse Forbidden(string message = "Forbidden")
        {
            return new ApiResponse("403", message, null);
        }

        public static ApiResponse InternalServerError(string message = "Internal Server Error")
        {
            return new ApiResponse("500", message, null);
        }
    }
} 