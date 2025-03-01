using API.Models;

namespace API.Services;

public class FileService
{
    private readonly string _folderPath = @"../BakalarkaWpf/bin/Debug/net8.0-windows/data";
    public FileService()
    {

    }

    public List<FileItem> ListAllTopFiles(string path)
    {
        List<FileItem> list = new List<FileItem>();
        var filePaths = Directory.EnumerateFiles(path);
        var directoryPaths = Directory.EnumerateDirectories(path);
        foreach (var directory in directoryPaths)
        {
            var item = new FileItem()
            {
                IsDirectory = true,
                Name = Path.GetFileName(directory),
                Path = Path.GetFullPath(directory),
                SubItems = null
            };
            list.Add(item);
        }
        foreach (var file in filePaths)
        {
            var item = new FileItem()
            {
                IsDirectory = false,
                Name = Path.GetFileName(file),
                Path = Path.GetFullPath(file),
                SubItems = null
            };
            list.Add(item);
        }
        return list;
    }
}
