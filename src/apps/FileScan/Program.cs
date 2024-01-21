using System;
using System.IO;
using System.Linq;
using Spectre.Console;

namespace FileScan;

internal static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            PrintHelp();
            return;
        }

        var arg = args.First();
        if (arg is "-h" or "--help")
        {
            PrintHelp();
            return;
        }

        if (!Directory.Exists(arg))
        {
            AnsiConsole.MarkupLine($"Directory [blue]{arg}[/] was [red]not[/] found.");
            return;
        }

        new DirectoryScanner(arg).Scan();
    }

    private static void PrintHelp()
    {
        Console.WriteLine("File Scan");
        Console.WriteLine("Command line program that finds the disk usage of files/folders in specified path");
        AnsiConsole.MarkupLine("Usage: [blue]FileScan <PATH>[/]");
        Console.WriteLine("Options:");
        AnsiConsole.MarkupLine("    [blue]<PATH>[/]\tPath of folder to scan");
        AnsiConsole.MarkupLine("    [blue]-h, --help[/]\tPrint help information");
    }
}