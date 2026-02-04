// TenantManagementService/Services/BillingHttpClient.cs
using SharedKernel.Services;
using SharedKernel.Dto;
using System.Text.Json;
using System.Text;

namespace SharedKernel.Services
{
    public class BillingHttpClient : IBillingClient
    {
        private readonly HttpClient _httpClient;

        public BillingHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task GenerateInvoiceAsync(GenerateInvoiceRequest dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/api/billing/generate-invoice", content);
        }
        public async Task<InvoiceResponse> GetInvoiceAsync(Guid invoiceId)
        {
            var response = await _httpClient.GetAsync($"/api/billing/invoices/{invoiceId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        }

        public async Task<List<InvoiceResponse>> GetInvoicesByTenantAsync(Guid tenantId)
        {
            var response = await _httpClient.GetAsync($"/api/billing/invoices/{tenantId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<InvoiceResponse>>();
        }

        public async Task ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/api/billing/process-payment", content);
        }
    }
}