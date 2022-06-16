namespace Coingate.Net
{
    public class Order
    {
        public int OrderId { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string ReceiveCurrency { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CallbackUrl { get; set; }
        public string CancelUrl { get; set; }
        public string SuccessUrl { get; set; }
    }
}
