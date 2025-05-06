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
            return null;
        }
    }
    public async Task<bool> UploadFileAsync(string localFilePath, string targetDirectoryPath)
    {
        try
        {
            if (!File.Exists(localFilePath))
            {
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
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public async Task<byte[]> GetFileAsync(string path)
    {
        var url = $"{_apiBaseUrl}File/file?path={Uri.EscapeDataString(path)}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task<Pdf> GetOcrAsync(string path, int height, int width)
    {
        try
        {
            var url = $"{_apiBaseUrl}File/ocr?path={Uri.EscapeDataString(path)}&height={height}&width={width}";
            var result = await _httpClient.GetFromJsonAsync<Pdf>(url);
            return result;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
    public async Task<bool> CopyItemAsync(string selectedItem, string destination)
    {
        try
        {
            var url = $"{_apiBaseUrl}File/copy?selectedItem={Uri.EscapeDataString(selectedItem)}&destination={Uri.EscapeDataString(destination)}";
            var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> MoveItemAsync(string selectedItem, string destination)
    {
        try
        {
            var url = $"{_apiBaseUrl}File/move?selectedItem={Uri.EscapeDataString(selectedItem)}&destination={Uri.EscapeDataString(destination)}";
            var response = await _httpClient.PutAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> DeleteItemAsync(string selectedItem)
    {
        try
        {
            var url = $"{_apiBaseUrl}File/delete?selectedItem={Uri.EscapeDataString(selectedItem)}";
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> CreateFolderAsync(string directoryPath)
    {
        try
        {
            var url = $"{_apiBaseUrl}File/create-folder";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("directoryPath", directoryPath)
            });

            var response = await _httpClient.PostAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    public async Task<bool> RenameItemAsync(string originalPath, string newName)
    {
        try
        {
            var url = $"{_apiBaseUrl}File/rename?originalPath={Uri.EscapeDataString(originalPath)}&newName={Uri.EscapeDataString(newName)}";
            var response = await _httpClient.PutAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}
