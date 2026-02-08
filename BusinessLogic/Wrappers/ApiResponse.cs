using System.Text.Json.Serialization;

namespace BusinessLogic.Wrappers
{
    public class ApiResponse<T>
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public int StatusCode { get; set; }

        public ApiResponse() { }

        public ApiResponse(T data, string message = null)
        {
            Succeeded = true;
            Message = message;
            Data = data;
            StatusCode = 200;
        }

        public ApiResponse(bool succeeded, string message, int statusCode)
        {
            Succeeded = succeeded;
            Message = message;
            StatusCode = statusCode;
        }
    }
}
