using System.Text.Json.Serialization;

namespace PaymentsServiceForModulbank.API.Requests
{
    public class ProviderRequest
    {
        [JsonPropertyName("operationId")]
        public string OperationId { get; set; } = string.Empty;
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;
    }
}
