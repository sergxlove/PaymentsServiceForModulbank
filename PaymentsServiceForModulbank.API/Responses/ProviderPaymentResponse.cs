using System.Text.Json.Serialization;

namespace PaymentsServiceForModulbank.API.Responses
{
    public class ProviderPaymentResponse
    {
        [JsonPropertyName("providerPaymentId")]
        public string ProviderPaymentId { get; set; } = string.Empty;
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
