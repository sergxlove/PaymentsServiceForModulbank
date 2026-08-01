namespace PaymentsServiceForModulebank.Core.Models
{
    public enum OperationStatus
    {
        CREATED,
        PROCESSING,
        COMPLETED,
        REJECTED
    }
    public class Operations
    {
        public string OperationId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public OperationStatus Status { get; set; }

        public string? ProviderPaymentId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int RetryCount { get; set; }
    }
}
