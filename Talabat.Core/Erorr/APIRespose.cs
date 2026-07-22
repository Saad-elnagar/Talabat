
namespace Talabat.Erorr
{
    public class APIRespose
    {
        
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public APIRespose(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        private string? GetDefaultMessageForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                401 => "Unauthorized",
                404 => "Not Found",
                500 => "Internal Server Error",
                _ => null
            };

        }
    }
}
