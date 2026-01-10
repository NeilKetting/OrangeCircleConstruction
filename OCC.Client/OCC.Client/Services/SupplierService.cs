using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly HttpClient _httpClient;

        public SupplierService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Supplier>> GetSuppliersAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Supplier>>("api/Suppliers") ?? new List<Supplier>();
        }

        public async Task<Supplier?> GetSupplierAsync(Guid id)
        {
             return await _httpClient.GetFromJsonAsync<Supplier>($"api/Suppliers/{id}");
        }

        public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Suppliers", supplier);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Supplier>() ?? supplier;
        }

        public async Task UpdateSupplierAsync(Supplier supplier)
        {
             var response = await _httpClient.PutAsJsonAsync($"api/Suppliers/{supplier.Id}", supplier);
             response.EnsureSuccessStatusCode();
        }

        public async Task DeleteSupplierAsync(Guid id)
        {
             var response = await _httpClient.DeleteAsync($"api/Suppliers/{id}");
             response.EnsureSuccessStatusCode();
        }
    }
}
