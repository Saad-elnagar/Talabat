// Talabat.Core/Exceptions/CheckoutSessionException.cs
namespace Talabat.Core.Exceptions
{
    public class CheckoutSessionException : Exception
    {
        public CheckoutSessionException(string message, Exception inner)
            : base(message, inner) { }
    }
}