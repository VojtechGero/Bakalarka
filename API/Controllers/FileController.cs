using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly ILogger<SearchController> _logger;
    private readonly FileService _fileService;
    public FileController(ILogger<SearchController> logger)
    {
        _logger = logger;
        _fileService = new FileService();
    }
    [HttpGet("list")]
    public List<FileItem> GetTopLevel(string path)
    {
        var FileItems = _fileService.ListAllTopItems(path);
        return FileItems;
    }
    [HttpGet("structure")]
    public FileItem GetStructure()
    {
        var FileItems = _fileService.ListAllItems();
        return FileItems;
    }
}
