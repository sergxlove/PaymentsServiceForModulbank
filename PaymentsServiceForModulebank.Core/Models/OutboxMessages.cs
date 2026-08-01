namespace PaymentsServiceForModulebank.Core.Models
{
    public class OutboxMessages
    {
        public Guid Id { get; set; }
        public string OperationId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int RetryCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
