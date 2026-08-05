namespace PaymentsServiceForModulebank.Core.Models
{
    public class Operations
    {
        public string OperationId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string? ProviderPaymentId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
