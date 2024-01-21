using Spectre.Console;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace FileScan;

public class DirectoryScanner(string directory)
{
    public void Scan()
    {
        if (!Directory.Exists(directory))
        {
            AnsiConsole.MarkupLine($"[blue]{directory}[/] does [red]not[/] exist");
            return;
        }

        if (directory == Path.GetPathRoot(directory))
        {
            AnsiConsole.MarkupLine($"[blue]{directory}[/] is [red]root[/] and cannot be scanned");
            return;
        }

        List<ScanItem> scanItems = [];

        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            scanItems.Add(new ScanItem()
            {
                Path = subDirectory,
                Name = subDirectory.Replace(directory, ""),
                Size = GetDirectorySize(subDirectory),
                IsDirectory = true,
            });
        }

        foreach (var file in Directory.GetFiles(directory))
        {
            scanItems.Add(new ScanItem()
            {
                Path = file,
                Name = file.Replace(directory, ""),
                Size = GetFileSize(file),
                IsDirectory = false,
            });
        }

        var sortedItems = scanItems.OrderByDescending(i => i.IsDirectory).ThenByDescending(i => i.Size).ToList();

        var parent = Path.GetFullPath(Path.Combine(directory, ".."));
        if (parent != Path.GetPathRoot(directory))
        {
            sortedItems.Insert(0, new ScanItem()
            {
                Path = parent,
                Name = "..",
                Size = 0,
                IsDirectory = true,
            });
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<ScanItem>()
                .Title($"Directory [blue]{directory}[/]")
                .PageSize(15)
                .AddChoices(sortedItems)
                .UseConverter(x => x.GetDisplay())
        );

        if (selected.IsDirectory)
        {
            new DirectoryScanner(selected.Path).Scan();
        }
    }

    private double GetDirectorySize(string path)
    {
        var size = 0.0;

        try
        {
            foreach (var file in Directory.GetFiles(path))
            {
                size += GetFileSize(file);
            }

            foreach (var subDirectory in Directory.GetDirectories(path))
            {
                size += GetDirectorySize(subDirectory);
            }
        }
        catch
        {
            // ignored
        }

        return size;
    }

    private double GetFileSize(string path)
    {
        return new FileInfo(path).Length;
    }
}