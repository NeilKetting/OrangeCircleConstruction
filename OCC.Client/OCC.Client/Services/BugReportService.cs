using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class BugReportService : IBugReportService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;

        public BugReportService(HttpClient httpClient, IAuthService authService)
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

        public async Task SubmitBugAsync(BugReport report)
        {
            EnsureAuthorization();
            var response = await _httpClient.PostAsJsonAsync("api/BugReports", report);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<BugReport>> GetBugReportsAsync()
        {
            EnsureAuthorization();
            return await _httpClient.GetFromJsonAsync<List<BugReport>>("api/BugReports") ?? new List<BugReport>();
        }
    }
}
