using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq;

namespace OCC.Client.Services
{
    public abstract class BaseApiService<T> : IRepository<T> where T : class
    {
        protected readonly HttpClient _httpClient;
        protected readonly IAuthService _authService;
        
        /// <summary>
        /// The resource name, e.g. "Projects" or "Users"
        /// </summary>
        protected abstract string ApiEndpoint { get; }

        public BaseApiService(IAuthService authService)
        {
            _authService = authService;
            _httpClient = new HttpClient();
            // TODO: Move base address to configuration/settings
            _httpClient.BaseAddress = new Uri("http://102.39.20.146:8081/"); 
        }

        protected virtual void EnsureAuthorization()
        {
            var token = _authService.AuthToken;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            EnsureAuthorization();
            try 
            {
                var result = await _httpClient.GetFromJsonAsync<IEnumerable<T>>($"api/{ApiEndpoint}");
                return result ?? Enumerable.Empty<T>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Get Error: {ex.Message}");
                // Return empty list on failure to prevent crash
                return Enumerable.Empty<T>();
            }
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            EnsureAuthorization();
            try
            {
                return await _httpClient.GetFromJsonAsync<T>($"api/{ApiEndpoint}/{id}");
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"API GetById Error: {ex.Message}");
                return null;
            }
        }

        public virtual async Task AddAsync(T entity)
        {
            EnsureAuthorization();
            await _httpClient.PostAsJsonAsync($"api/{ApiEndpoint}", entity);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            EnsureAuthorization();
            // Use PUT to update. Backend should handle identifying the entity from the body.
            await _httpClient.PutAsJsonAsync($"api/{ApiEndpoint}", entity);
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            EnsureAuthorization();
            await _httpClient.DeleteAsync($"api/{ApiEndpoint}/{id}");
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            // Note: For true efficiency, this should push the query to the API (e.g. OData).
            // For now, valid implementation is to fetch all and filter client-side.
            var allItems = await GetAllAsync();
            return allItems.Where(predicate.Compile());
        }
    }
}
