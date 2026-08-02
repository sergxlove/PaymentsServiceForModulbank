namespace PaymentsServiceForModulebank.Core.Models
{
    public class Events
    {
        public Guid Id { get; set; }
        public string OperationId { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }

        public Events Init(string operationId)
        {
            return new Events
            {
                Id = Guid.NewGuid(),
                OperationId = operationId,
                EventId = 1,
                FromStatus = null,
                ToStatus = "CREATED",
                Message = "Operation created",
                OccurredAt = DateTime.UtcNow
            }; 
        }

        public void Update(string newStatus, string newMessage)
        {
            EventId = EventId + 1;
            FromStatus = ToStatus;
            ToStatus = newStatus;
            Message = newMessage;
        }
    }
}
