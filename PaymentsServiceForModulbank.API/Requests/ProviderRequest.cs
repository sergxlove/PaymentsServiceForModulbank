using System.Text.Json.Serialization;

namespace PaymentsServiceForModulbank.API.Requests
{
    public class ProviderRequest
    {
        [JsonPropertyName("operationId")]
        public string operationId { get; set; } = string.Empty;
        [JsonPropertyName("amount")]
        public string amount { get; set; } = string.Empty;
        [JsonPropertyName("currency")]
        public string currency { get; set; } = string.Empty;
    }
}
