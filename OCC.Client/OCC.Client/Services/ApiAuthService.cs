using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OCC.Shared.DTOs;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class ApiAuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private User? _currentUser;
        private string? _authToken;

        public ApiAuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://102.39.20.146:8081/"); // Production Server
        }

        public User? CurrentUser => _currentUser;
        public string? AuthToken => _authToken;
        public bool IsAuthenticated => _currentUser != null;

        public async Task<(bool Success, string ErrorMessage)> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", new LoginRequest { Email = email, Password = password });
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result != null)
                    {
                        _currentUser = result.User;
                        _authToken = result.Token;
                        // Add token to default headers for future requests
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
                        return (true, string.Empty);
                    }
                }
                else
                {
                    // Read error message from API (e.g., "Account pending approval...")
                    var errorContent = await response.Content.ReadAsStringAsync();
                    // Clean up quotes if it's a JSON string
                    return (false, errorContent.Trim('"'));
                }
            }
            catch (Exception ex)
            {
                // Log error
                return (false, "Connection error: " + ex.Message);
            }
            return (false, "Unknown error occurred.");
        }

        public async Task<bool> RegisterAsync(User user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/register", user);
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task LogoutAsync()
        {
            _currentUser = null;
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            return Task.CompletedTask;
        }

        private class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public User User { get; set; } = new();
        }
    }
}
