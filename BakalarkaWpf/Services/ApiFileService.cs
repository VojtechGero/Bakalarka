using BakalarkaWpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
    public async Task<bool> UploadFileAsync(string localFilePath, string targetDirectoryPath)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
                Console.WriteLine("File not found.");
                return false;
            }

            using var fileStream = File.OpenRead(localFilePath);
            var fileName = Path.GetFileName(localFilePath);

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "file", fileName);
            content.Add(new StringContent(targetDirectoryPath), "path");

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}File/upload", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("File uploaded successfully.");
                return true;
            }
            else
            {
                Console.WriteLine($"Upload failed with status code: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
            return false;
        }
    }
}
