using BakalarkaWpf.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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
            Console.WriteLine($"Error in SearchAsync: {ex.Message}");
            return new List<FileResults>();
        }
    }

    public async Task<List<SearchResult>> GetFileResults(string query, string fileName)
    {
        try
        {
            var url = $"{_apiBaseUrl}Search/result?query={Uri.EscapeDataString(query)}&fileName={Uri.EscapeDataString(fileName)}";
            var results = await _httpClient.GetFromJsonAsync<List<SearchResult>>(url);
            return results ?? new List<SearchResult>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetFileResults: {ex.Message}");
            return new List<SearchResult>();
        }
    }
}
