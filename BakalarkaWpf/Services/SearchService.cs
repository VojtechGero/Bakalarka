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
                var boxTexts = page.OcrBoxes.Select(b => b.Text ?? string.Empty).ToList();
                string combinedText = string.Join(" ", boxTexts);

                int matchIndex = combinedText.IndexOf(string.Join(" ", queryWords), StringComparison.OrdinalIgnoreCase);

                while (matchIndex >= 0)
                {
                    int currentCharIndex = 0;

                    for (int i = 0; i < boxTexts.Count; i++)
                    {
                        string boxText = boxTexts[i];
                        if (matchIndex >= currentCharIndex && matchIndex < currentCharIndex + boxText.Length)
                        {
                            results.Add(new SearchResult
                            {
                                FilePath = pdf.Path,
                                PageNumber = page.pageNum,
                                MatchedText = combinedText.Substring(matchIndex, query.Length),
                                MatchIndex = matchIndex,
                                BoxIndex = i
                            });
                            break;
                        }

                        currentCharIndex += boxText.Length + 1;
                    }

                    matchIndex = combinedText.IndexOf(string.Join(" ", queryWords), matchIndex + query.Length, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        return results;
    }
}