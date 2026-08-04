namespace PaymentsServiceForModulebank.Core.Models
{
    public class Events
    {
        public Guid Id { get; set; }
        public string OperationId { get; set; } = string.Empty;
        public int EventId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }

        public static Events Init(string operationId)
        {
            return new Events
            {
                Id = Guid.NewGuid(),
                OperationId = operationId,
                EventId = 1,
                Type = "CREATED",
                FromStatus = null,
                ToStatus = "CREATED",
                Message = "Operation created",
                OccurredAt = DateTime.UtcNow
            }; 
        }

        public void Update(string newType, string newStatus, string newMessage)
        {
            Id = Guid.NewGuid();
            EventId = EventId + 1;
            Type = newType;
            FromStatus = ToStatus;
            ToStatus = newStatus;
            Message = newMessage;
            OccurredAt = DateTime.UtcNow;
        }
    }
}
