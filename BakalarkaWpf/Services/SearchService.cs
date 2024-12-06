using BakalarkaWpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BakalarkaWpf.Services;



public class SearchService
{
    private readonly string _folderPath;

    public SearchService(string folderPath)
    {
        _folderPath = folderPath;
    }

    public async Task<List<SearchResult>> SearchAsync(string query)
    {
        var results = new List<SearchResult>();
        var jsonFiles = Directory.EnumerateFiles(_folderPath, "*.json");

        foreach (var file in jsonFiles)
        {
            var jsonContent = await File.ReadAllTextAsync(file);
            var pdf = JsonSerializer.Deserialize<Pdf>(jsonContent);

            if (pdf == null) continue;

            foreach (var page in pdf.Pages)
            {
                foreach (var box in page.OcrBoxes)
                {
                    if (box.Text != null)
                    {
                        int matchIndex = box.Text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
                        if (matchIndex >= 0)
                        {
                            results.Add(new SearchResult
                            {
                                FilePath = pdf.Path,
                                PageNumber = page.pageNum,
                                MatchedText = box.Text,
                                MatchIndex = matchIndex
                            });
                        }
                    }
                }
            }
        }

        return results;
    }
}

public class SearchResult
{
    public string FilePath { get; set; }
    public int PageNumber { get; set; }
    public string MatchedText { get; set; }
    public int MatchIndex { get; set; }
}

// Usage
// var searchService = new SearchService("path_to_folder");
// var results = await searchService.SearchAsync("search_term");
