using PaymentsServiceForModulebank.Core.Models;

namespace PaymentsServiceForModulbank.API.Responses
{
    public class OperationCreateResponse
    {
        public string OperationId { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public OperationStatus Status { get; set; }

        public string? ProviderPaymentId { get; set; }
    }
}
