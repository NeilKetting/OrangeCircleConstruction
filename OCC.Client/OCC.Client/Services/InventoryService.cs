using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;

        public InventoryService(HttpClient httpClient, IAuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        private void EnsureAuthorization()
        {
            var token = _authService.AuthToken;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<InventoryItem>> GetInventoryAsync()
        {
            EnsureAuthorization();
            return await _httpClient.GetFromJsonAsync<List<InventoryItem>>("api/Inventory") ?? new List<InventoryItem>();
        }

        public async Task<InventoryItem?> GetInventoryItemAsync(Guid id)
        {
             EnsureAuthorization();
             return await _httpClient.GetFromJsonAsync<InventoryItem>($"api/Inventory/{id}");
        }

        public async Task<InventoryItem> CreateItemAsync(InventoryItem item)
        {
            EnsureAuthorization();
            var response = await _httpClient.PostAsJsonAsync("api/Inventory", item);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InventoryItem>() ?? item;
        }

        public async Task UpdateItemAsync(InventoryItem item)
        {
             EnsureAuthorization();
             var response = await _httpClient.PutAsJsonAsync($"api/Inventory/{item.Id}", item);
             response.EnsureSuccessStatusCode();
        }
    }
}
