using BakalarkaWpf.Models;
using System.Text.Json;

namespace API.Services;
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
        var searchTerms = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (searchTerms.Length == 0)
            return results;

        var normalizedQuery = string.Join(" ", searchTerms).Trim();

        foreach (var jsonFile in Directory.EnumerateFiles(_folderPath, "*.json", SearchOption.AllDirectories))
        {
            var jsonContent = await File.ReadAllTextAsync(jsonFile);
            var pdf = JsonSerializer.Deserialize<Pdf>(jsonContent);
            if (pdf?.Pages == null) continue;

            var pdfPath = Path.GetFullPath(Path.ChangeExtension(jsonFile, ".pdf"));

            foreach (var page in pdf.Pages)
            {
                // Validate page number before processing
                if (page.pageNum < 1) continue;

                foreach (var (box, boxIndex) in page.OcrBoxes.Select((b, i) => (b, i)))
                {
                    var boxText = box.Text ?? string.Empty;
                    int searchPosition = 0;

                    while (true)
                    {
                        // Find case-insensitive match
                        var matchIndex = boxText.IndexOf(
                            normalizedQuery,
                            searchPosition,
                            StringComparison.OrdinalIgnoreCase
                        );

                        if (matchIndex < 0) break;

                        results.Add(new SearchResult
                        {
                            Query = query,
                            FilePath = pdfPath,
                            PageNumber = page.pageNum, // Now guaranteed ≥1
                            MatchedText = boxText.Substring(matchIndex, normalizedQuery.Length),
                            MatchIndex = matchIndex,
                            BoxIndex = boxIndex,
                            BoxSpan = 1
                        });

                        // Move search position forward
                        searchPosition = matchIndex + normalizedQuery.Length;
                    }
                }
            }
        }

        return results;
    }
    public async Task<List<SearchResult>> SearchFileAsync(string query, string filename)
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
                    int startBoxIndex = -1;
                    int boxSpan = 0;
                    int remainingMatchLength = query.Length;

                    for (int i = 0; i < boxTexts.Count; i++)
                    {
                        string boxText = boxTexts[i];
                        if (matchIndex >= currentCharIndex && matchIndex < currentCharIndex + boxText.Length)
                        {
                            startBoxIndex = i;
                            while (remainingMatchLength > 0 && i < boxTexts.Count)
                            {
                                int boxTextLength = boxTexts[i].Length;
                                remainingMatchLength -= Math.Min(remainingMatchLength, boxTextLength);
                                boxSpan++;
                                i++;
                            }

                            results.Add(new SearchResult
                            {
                                Query = query,
                                FilePath = pdf.Path,
                                PageNumber = page.pageNum,
                                MatchedText = combinedText.Substring(matchIndex, query.Length),
                                MatchIndex = matchIndex,
                                BoxIndex = startBoxIndex,
                                BoxSpan = boxSpan > 1 ? boxSpan - 1 : boxSpan
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