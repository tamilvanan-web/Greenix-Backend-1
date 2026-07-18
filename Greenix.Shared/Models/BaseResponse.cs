using System.Collections.Generic;

namespace Greenix.Shared.Models
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public BaseResponse()
        {
        }

        public BaseResponse(T data, string message = "")
        {
            Success = true;
            Data = data;
            Message = message;
        }

        public BaseResponse(string errorMessage)
        {
            Success = false;
            Errors.Add(errorMessage);
        }

        public BaseResponse(List<string> errors, string message = "")
        {
            Success = false;
            Errors = errors;
            Message = message;
        }
    }
}
