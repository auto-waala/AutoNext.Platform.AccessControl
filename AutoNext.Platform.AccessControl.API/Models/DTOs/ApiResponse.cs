namespace AutoNext.Platform.AccessControl.API.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int StatusCode { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = 200
            };
        }

        public static ApiResponse<T> Created(T data, string message = "Created successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = 201
            };
        }

        public static ApiResponse<T> Error(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> NotFound(string message = "Resource not found")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 404
            };
        }

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized access")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = 401
            };
        }
    }
}
