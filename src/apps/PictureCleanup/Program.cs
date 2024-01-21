using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Linux;
using Spectre.Console;

namespace PictureCleanup;

internal static class Program
{
    private static string _directory = string.Empty;
    private const int MaxSize = 2400;

    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            PrintHelp();
            return;
        }

        _directory = args.First();
        if (_directory is "-h" or "--help")
        {
            PrintHelp();
            return;
        }

        if (!Directory.Exists(_directory))
        {
            AnsiConsole.MarkupLine($"Directory [blue]{_directory}[/] was [red]not[/] found.");
            return;
        }

        var pictures = DirectorySearch(_directory);

        var folderChoices = new HashSet<string>();
        foreach (var picture in pictures)
        {
            folderChoices.Add(string.Join("/", picture.Split("/").SkipLast(1)));
        }

        var selectedFolders = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title($"Select [blue]folders[/] to [red]resize[/] containing pictures smaller:")
                .PageSize(20)
                .AddChoices(folderChoices)
                .NotRequired()
        );

        foreach (var picture in pictures)
        {
            var picturePath = Path.Combine(_directory, picture);

            var fileExtension = new FileInfo(picturePath).Extension;
            if (fileExtension != ".jpeg")
            {
                AnsiConsole.MarkupLine($"Attempting to convert [cyan]{picture}[/] to a [red]jpeg[/]...");

                var newPicturePath = picturePath.Replace(fileExtension, ".jpeg");
                new Command($"convert {picturePath} {newPicturePath}").Run();
                File.Delete(picturePath);
                picturePath = newPicturePath;
            }

            if (!selectedFolders.Any(f => picture.StartsWith(f))) continue;

            AnsiConsole.MarkupLine($"Checking size of [cyan]{picture}[/]...");

            var height = int.Parse(new Command($"identify -format %[h] {picturePath}").GetOutput());
            var width = int.Parse(new Command($"identify -format %[w] {picturePath}").GetOutput());

            if (height <= MaxSize && width <= MaxSize) continue;

            if (height > width)
            {
                AnsiConsole.MarkupLine($"    Image is [red]too tall[/] (height: {height})");
                new Command($"convert {picturePath} -resize x{MaxSize} {picturePath}").Run();
            }
            else
            {
                AnsiConsole.MarkupLine($"    Image is [red]too wide[/] (width: {width})");
                new Command($"convert {picturePath} -resize {MaxSize} {picturePath}").Run();
            }
        }
    }

    private static List<string> DirectorySearch(string directory)
    {
        var result = new List<string>();

        if (directory.Contains(".git"))
        {
            return result;
        }

        if (!Directory.Exists(directory))
        {
            return result;
        }

        foreach (var subDirectory in Directory.GetDirectories(directory))
        {
            result = result.Concat(DirectorySearch(subDirectory)).ToList();
        }

        foreach (var file in Directory.GetFiles(directory))
        {
            result.Add(file.Replace(_directory, "").TrimStart('/'));
        }

        return result;
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Picture Cleanup");
        Console.WriteLine("Command line program that resizes and renames pictures in specified path");
        AnsiConsole.MarkupLine("Usage: [blue]PictureCleanup <PATH>[/]");
        Console.WriteLine("Options:");
        AnsiConsole.MarkupLine("    [blue]<PATH>[/]\tPath of folder to cleanup");
        AnsiConsole.MarkupLine("    [blue]-h, --help[/]\tPrint help information");
    }
}