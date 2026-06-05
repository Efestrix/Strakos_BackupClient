using Strakos_BackupClient.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Strakos_BackupClient.Api
{
    public class ApiConfigRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiConfigRepository(string baseUrl)
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl.TrimEnd('/');
        }
        public async Task<List<BackupJob>> LoadJobsAsync(string computerUuid)
        {
            string url = $"{_baseUrl}/api/Computers/{computerUuid}/jobs";

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            List<BackupJob>? jobs = await _httpClient.GetFromJsonAsync<List<BackupJob>>(url, options);

            return jobs ?? new List<BackupJob>();
        }
    }
}
