namespace Taskify.Services.DTOs
{
	public class ApiResponse<T>
	{
		public string? Message { get; init; }
		public bool IsSuccessful { get; init; }
		public T? Data { get; init; }
		public int StatusCode { get; init; }
		public IEnumerable<ValidationError> Errors { get; init; } = Enumerable.Empty<ValidationError>();
	}

	public static class ApiResponseBuilder
	{
		public static ApiResponse<T> Success<T>(T data, string message = "Request successful", int statusCode = 200)
		{
			return new ApiResponse<T>
			{
				Message = message,
				StatusCode = statusCode,
				IsSuccessful = true,
				Data = data
			};
		}

		public static ApiResponse<T> Fail<T>(string message = "Request failed", int statusCode = 400, IEnumerable<ValidationError> errors = null!)
		{
			return new ApiResponse<T>
			{
				Message = message,
				StatusCode = statusCode,
				IsSuccessful = false,
				Data = default,
				Errors = errors ?? Enumerable.Empty<ValidationError>()

			};
		}

		public static ApiResponse<T> Custom<T>(string message, bool isSuccessful = false, T data = default!, int statusCode = 400, IEnumerable<ValidationError> errors = null!)
		{
			return new ApiResponse<T>
			{
				Message = message,
				StatusCode = statusCode,
				IsSuccessful = isSuccessful,
				Data = data,
				Errors = errors ?? Enumerable.Empty<ValidationError>()
			};
		}
	}

}
