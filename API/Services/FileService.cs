using API.Models;

namespace API.Services;

public class FileService
{
    private readonly string _folderPath = @"../BakalarkaWpf/bin/Debug/net8.0-windows/data";
    public FileService()
    {

    }

    public List<FileItem> ListAllTopItems(string path)
    {
        List<FileItem> list = new List<FileItem>();
        var filePaths = Directory.EnumerateFiles(path, "*.json");
        var directoryPaths = Directory.EnumerateDirectories(path);
        foreach (var directory in directoryPaths)
        {
            var item = new FileItem()
            {
                IsDirectory = true,
                Name = Path.GetFileName(directory),
                Path = directory,
                SubItems = null
            };
            list.Add(item);
        }
        foreach (var file in filePaths)
        {
            var item = new FileItem()
            {
                IsDirectory = false,
                Name = Path.ChangeExtension(Path.GetFileName(file), ".pdf"),
                Path = Path.ChangeExtension(file, ".pdf"),
                SubItems = null
            };
            list.Add(item);
        }
        return list;
    }
    public FileItem ListAllItems()
    {
        var item = GetFolderStructure(_folderPath);
        return item;
    }
    private FileItem GetFolderStructure(string path)
    {
        var folderName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(folderName))
        {
            folderName = path;
        }

        var folder = new FileItem
        {
            Name = folderName,
            Path = path,
            IsDirectory = true,
            SubItems = new List<FileItem>()
        };

        foreach (var dir in Directory.GetDirectories(path))
        {
            folder.SubItems.Add(GetFolderStructure(dir));
        }

        foreach (var file in Directory.GetFiles(path)
                     .Where(x => x.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)))
        {
            folder.SubItems.Add(new FileItem
            {
                Name = Path.GetFileName(file),
                Path = file,
                IsDirectory = false,
                SubItems = null
            });
        }

        return folder;
    }
}
