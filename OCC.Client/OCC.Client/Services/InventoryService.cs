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

        public InventoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<InventoryItem>> GetInventoryAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<InventoryItem>>("api/Inventory") ?? new List<InventoryItem>();
        }

        public async Task<InventoryItem?> GetInventoryItemAsync(Guid id)
        {
             return await _httpClient.GetFromJsonAsync<InventoryItem>($"api/Inventory/{id}");
        }

        public async Task<InventoryItem> CreateItemAsync(InventoryItem item)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Inventory", item);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InventoryItem>() ?? item;
        }

        public async Task UpdateItemAsync(InventoryItem item)
        {
             var response = await _httpClient.PutAsJsonAsync($"api/Inventory/{item.Id}", item);
             response.EnsureSuccessStatusCode();
        }
    }
}
