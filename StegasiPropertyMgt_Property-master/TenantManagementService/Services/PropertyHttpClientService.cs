// TenantManagementService/Services/PropertyHttpClientService.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SharedKernel.Models;
using SharedKernel.Dto;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using SharedKernel.Services;
using Newtonsoft.Json;


namespace TenantManagementService.Services
{
    public class PropertyHttpClientService : IPropertyService
    {
        private readonly HttpClient _httpClient;

        public PropertyHttpClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Property> GetPropertyByIdAsync(Guid id, string token = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/property/{id}");
            
            // Only add Authorization header if token is valid
            if (!string.IsNullOrEmpty(token) && !token.Contains("your_jwt_token_here") && !token.Contains("REPLACE_WITH_ACTUAL_JWT_TOKEN"))
            {
                var cleanToken = token.Replace("Bearer ", "").Trim();
                if (!string.IsNullOrEmpty(cleanToken))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cleanToken);
                }
            }
            
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new KeyNotFoundException($"Property with ID {id} not found.");
            }
            
            var x = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response JSON: {x}");

            return await response.Content.ReadFromJsonAsync<Property>();
        }
        
        public async Task<PropertyDetailResponse> GetPropertyDetailByIdAsync(Guid id, string token = null)
        {
            try
            {
                // Call the regular property endpoint instead of a detail endpoint
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/property/{id}");
                
                // Only add Authorization header if token is valid
                if (!string.IsNullOrEmpty(token) && !token.Contains("your_jwt_token_here") && !token.Contains("REPLACE_WITH_ACTUAL_JWT_TOKEN"))
                {
                    var cleanToken = token.Replace("Bearer ", "").Trim();
                    if (!string.IsNullOrEmpty(cleanToken))
                    {
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cleanToken);
                    }
                }
                
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Failed to fetch property details.");
                }
                
                // Get the property from the regular endpoint
                var propertyResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Property>>();
                if (propertyResponse?.Data == null)
                {
                    throw new InvalidOperationException("Failed to fetch property details.");
                }
                
                var property = propertyResponse.Data;
                
                // Map Property to PropertyDetailResponse
                var propertyDetail = new PropertyDetailResponse
                {
                    Id = property.Id,
                    Name = property.Name,
                    Address = property.Address,
                    OwnerId = property.OwnerId,
                    PropertyManagerId = property.PropertyManagerId,
                    YearOfCommissionOrPurchase = property.YearOfCommissionOrPurchase,
                    FairValue = property.FairValue,
                    InsurableValue = property.InsurableValue,
                    OwnershipStatus = property.OwnershipStatus,
                    SalePrice = property.SalePrice ?? 0,
                    Type = property.PropertyType,
                    Status = property.ApprovalStatus,
                    RentPrice = property.RentPrice,
                    IsRentable = property.IsRentable,
                    Images = new List<string>(), // Initialize empty list for now
                    PrimaryImageUrl = null // Initialize as null for now
                };
                
                return propertyDetail;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Failed to fetch property details", ex);
            }
        }



        public async Task<List<Property>> GetAvailablePropertiesAsync()
        {
            var response = await _httpClient.GetAsync("/api/property/available-properties");

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Failed to fetch available properties.");
            }

            return await response.Content.ReadFromJsonAsync<List<Property>>() ?? new List<Property>(); // Handle null case
        }
        public async Task UpdatePropertyAsync(Property property)
        {
            var response = await _httpClient.PutAsJsonAsync($"/{property.Id}", property);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to update property with ID {property.Id}.");
            }
        }


        // Implement all IPropertyService methods:
        public async Task CreatePropertyAsync(CreatePropertyDto dto)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/api/property/create", content);
        }

        public async Task<Property> CreatePropertyAsync(CreatePropertyDto dto, string userId)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/property/create", content);
            return await response.Content.ReadFromJsonAsync<Property>();
        }



        public async Task<List<Property>> GetAllPropertiesAsync()
        {
            var response = await _httpClient.GetAsync("/api/property/all");
            return await response.Content.ReadFromJsonAsync<List<Property>>();
        }

        /*public async Task UpdatePropertyAsync(Guid id, CreatePropertyDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PutAsync($"/api/property/{id}", content);
        }
        

        public async Task DeletePropertyAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"/api/property/{id}");
        }*/
        public async Task<List<Property>> GetPropertiesByTypeAsync(string propertyType)
        {
            var response = await _httpClient.GetAsync($"/api/property/type/{propertyType}");
            return await response.Content.ReadFromJsonAsync<List<Property>>();
        }
        public async Task<List<Property>> GetPropertiesByFilterAsync(string status, string type)
        {
            var response = await _httpClient.GetAsync($"/api/property/filter?status={status}&type={type}");
            return await response.Content.ReadFromJsonAsync<List<Property>>();
        }
        public async Task DeletePropertyAsync(Guid id, string userId)
        {
            await _httpClient.DeleteAsync($"/api/property/{id}");

        }
        public async Task UpdatePropertyAsync(Guid id, CreatePropertyDto dto, string userId)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PutAsync($"/api/property/{id}", content);
        }

    }
}