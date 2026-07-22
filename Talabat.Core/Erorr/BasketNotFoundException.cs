// Talabat.Core/Exceptions/BasketNotFoundException.cs
namespace Talabat.Core.Exceptions
{
    public class BasketNotFoundException : Exception
    {
        public BasketNotFoundException(string basketId)
            : base($"Basket with id '{basketId}' was not found.") { }
    }
}