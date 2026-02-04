using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SharedKernel.Models;
using Microsoft.Extensions.Logging;

namespace BillingService.Services
{
    public class PropertyClient : IPropertyClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PropertyClient> _logger;

        public PropertyClient(HttpClient httpClient, ILogger<PropertyClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Property> GetPropertyAsync(Guid propertyId)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<Property>($"api/properties/{propertyId}");
                return response ?? throw new InvalidOperationException($"Property with ID {propertyId} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting property {PropertyId}", propertyId);
                throw;
            }
        }

        public async Task<Property> GetPropertyByIdAsync(Guid propertyId)
        {
            return await GetPropertyAsync(propertyId);
        }

        public async Task<bool> ValidatePropertyAsync(Guid propertyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/properties/{propertyId}/validate");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating property {PropertyId}", propertyId);
                return false;
            }
        }
    }
}
