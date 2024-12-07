using BakalarkaWpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string[] queryWords = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var pdf = JsonSerializer.Deserialize<Pdf>(jsonContent);

            if (pdf == null) continue;

            foreach (var page in pdf.Pages)
            {
                // Combine all text from boxes into a single searchable stream
                var boxTexts = page.OcrBoxes.Select(b => b.Text ?? string.Empty).ToList();
                string combinedText = string.Join(" ", boxTexts);

                // Search for the query in the combined text
                int matchIndex = combinedText.IndexOf(string.Join(" ", queryWords), StringComparison.OrdinalIgnoreCase);

                if (matchIndex >= 0)
                {
                    // Map matchIndex back to the specific boxes
                    int currentCharIndex = 0;

                    for (int i = 0; i < boxTexts.Count; i++)
                    {
                        string boxText = boxTexts[i];
                        if (matchIndex >= currentCharIndex && matchIndex < currentCharIndex + boxText.Length)
                        {
                            // The match starts in this box
                            results.Add(new SearchResult
                            {
                                FilePath = pdf.Path,
                                PageNumber = page.pageNum,
                                MatchedText = combinedText.Substring(matchIndex, query.Length),
                                MatchIndex = matchIndex,
                                BoxIndex = i // Include the index of the box where the match starts
                            });
                            break; // Stop after finding the first match
                        }

                        currentCharIndex += boxText.Length + 1; // +1 for the space added during concatenation
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
    public int BoxIndex { get; set; }
}

// Usage
// var searchService = new SearchService("path_to_folder");
// var results = await searchService.SearchAsync("search_term");
