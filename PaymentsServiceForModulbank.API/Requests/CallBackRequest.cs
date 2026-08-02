namespace PaymentsServiceForModulbank.API.Requests
{
    public class CallBackRequest
    {
        public string ProviderPaymentId { get; set; } = string.Empty;
        public string OperationId { get; set; } = string.Empty; 
        public string Result { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; } 
    }
}
