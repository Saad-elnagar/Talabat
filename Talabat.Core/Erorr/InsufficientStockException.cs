namespace Talabat.Core.Exceptions
{
    public class InsufficientStockException : Exception
    {
        public string ProductName { get; }
        public int Requested { get; }
        public int Available { get; }

        public InsufficientStockException(string productName, int requested, int available)
            : base($"Insufficient stock for '{productName}'. Requested: {requested}, Available: {available}.")
        {
            ProductName = productName;
            Requested = requested;
            Available = available;
        }
    }
}