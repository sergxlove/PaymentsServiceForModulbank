namespace PaymentsServiceForModulbank.API.Requests
{
    public class OperationCreateRequest
    {
        public string OperationId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
