using BakalarkaWpf.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace BakalarkaWpf.Services;
public class ApiSearchService
{

    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "https://localhost:7008/";

    public ApiSearchService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<List<FileResults>> SearchAsync(string query)
    {
        try
        {
            var url = $"{_apiBaseUrl}Search/results?query={Uri.EscapeDataString(query)}";
            var results = await _httpClient.GetFromJsonAsync<List<FileResults>>(url);
            return results ?? new List<FileResults>();
        }
        catch (Exception ex)
        {
            return new List<FileResults>();
        }
    }

    public async Task<List<SearchResult>> GetFileResults(string query, string fileName)
    {
        try
        {
            var url = $"{_apiBaseUrl}Search/result?query={Uri.EscapeDataString(query)}&fileName={Uri.EscapeDataString(fileName)}";
            var restultjson = await _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync();
            if (restultjson != null)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var results = JsonSerializer.Deserialize<List<SearchResult>>(restultjson, options);
                return results;
            }
            return new List<SearchResult>();
        }
        catch (Exception ex)
        {
            return new List<SearchResult>();
        }
    }
}
