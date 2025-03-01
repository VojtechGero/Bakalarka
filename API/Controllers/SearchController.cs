using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using FileResults = API.Models.FileResults;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController : ControllerBase
{

    private readonly ILogger<SearchController> _logger;
    private SearchService _searchService;

    public SearchController(ILogger<SearchController> logger)
    {
        _logger = logger;
        _searchService = new SearchService();
    }

    [HttpGet("results")]
    public async Task<IEnumerable<FileResults>> GetResults(string query)
    {
        var results = await _searchService.SearchAsync(query);
        var fileResults = results
        .GroupBy(sr => sr.FilePath)
        .Select(group => new FileResults
        {
            Query = query,
            FilePath = group.Key,
            OccurrenceCount = group.Count()
        })
        .ToList();
        return fileResults;
    }
    [HttpGet("result")]
    public async Task<List<SearchResult>> GetResult(string query, string fileName, int occurrenceCount)
    {
        var results = await _searchService.SearchAsync(query);
        var fileResults = results
        .Where(sr => sr.FilePath == fileName)
        .ToList();
        return fileResults;
    }
    /*
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded or file is empty.");
        }

        try
        {
            Directory.CreateDirectory(_workingFolder);

            var filePath = Path.Combine(_workingFolder, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation($"File uploaded successfully: {filePath}");
            return Ok(new { Message = "File uploaded successfully", FileName = file.FileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading the file.");
            return StatusCode(500, "Internal server error while uploading the file.");
        }
    }
    */
}

