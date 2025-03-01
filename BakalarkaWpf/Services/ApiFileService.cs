using BakalarkaWpf.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BakalarkaWpf.Services;

public class ApiFileService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = @"https://localhost:7008/";
    public ApiFileService()
    {
        _httpClient = new HttpClient();
    }
    public async Task<List<FileItem>> GetTopLevelItems(string path)
    {
        try
        {
            var url = $"{_apiBaseUrl}File/list?path={Uri.EscapeDataString(path)}";
            var results = await _httpClient.GetFromJsonAsync<List<FileItem>>(url);
            return results ?? new List<FileItem>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTopLevelItems: {ex.Message}");
            return new List<FileItem>();
        }
    }
    public async Task<FileItem?> GetTopAllItems()
    {
        try
        {
            var url = $"{_apiBaseUrl}File/structure";
            var results = await _httpClient.GetFromJsonAsync<FileItem>(url);
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTopLevelItems: {ex.Message}");
            return null;
        }
    }
}
