namespace Talabat.Erorr
{
    public class ValidationErorrRespose : APIRespose
    {
        public IEnumerable<string> Errors { get; set; }
        public ValidationErorrRespose(int statusCode ,string massage ,IEnumerable<string> errors) : base(statusCode)
        {
            Errors = errors;
        }
    }
}
