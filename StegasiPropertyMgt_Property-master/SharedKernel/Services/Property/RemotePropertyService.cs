// SharedKernel/Services/RemotePropertyService.cs
/*using System.Net.Http.Json;
using System.Threading.Tasks;
using SharedKernel.Services;
using SharedKernel.Models;
using SharedKernel.Dto;
using System;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text;

namespace SharedKernel.Services.Remote
{
    public class RemotePropertyService : IPropertyService
    {
        private readonly HttpClient _httpClient;

        public RemotePropertyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreatePropertyAsync(CreatePropertyDto dto,string userId)
        {
            // Example: Forward the request to PropertyManagementService via HTTP
            var jsonContent = JsonSerializer.Serialize(dto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/api/property/create", content);
        }
        public async Task<List<Property>> GetAllPropertiesAsync()
        {
            var response = await _httpClient.GetAsync("/api/property/all");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Property>>();
        }

        public async Task UpdatePropertyAsync(Guid id, CreatePropertyDto dto, string userId)
        {
            var jsonContent = JsonSerializer.Serialize(dto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            await _httpClient.PutAsync($"/api/property/{id}", content);
        }

        public async Task<Property> GetPropertyByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"/{id}");

            if (!response.IsSuccessStatusCode)
            {
                throw new KeyNotFoundException($"Property with ID {id} not found.");
            }

            return await response.Content.ReadFromJsonAsync<Property>();
        }
        public async Task<List<Property>> GetAvailablePropertiesAsync(){
            var response = await _httpClient.GetAsync($"/api/property/available-properties");
            if (!response.IsSuccessStatusCode)
            {
                throw new KeyNotFoundException($"Properties not found.");
            }
            return await response.Content.ReadFromJsonAsync<List<Property>>();  
            //await  response.Content.ReadFromJsonAsync<Property>();
        }
        public async Task UpdatePropertyAsync(Property property){
            var response = await _httpClient.PutAsJsonAsync($"/api/property/{property.Id}", property);
            if (!response.IsSuccessStatusCode)
            {
                throw new KeyNotFoundException($"Property with ID {property.Id} not found.");
            }
            await response.Content.ReadFromJsonAsync<Property>();
        }
        public async Task DeletePropertyAsync(Guid id){
            var response = await _httpClient.DeleteAsync($"/api/property/{id}");
            if (!response.IsSuccessStatusCode)
            {
                throw new KeyNotFoundException($"Property with ID {id} not found.");
            }
            await response.Content.ReadFromJsonAsync<Property>();
        }
    }
}

*/