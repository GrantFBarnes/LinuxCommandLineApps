using System;
using System.Text;

namespace FileScan;

public class ScanItem
{
    public string Path;
    public string Name;
    public double Size;
    public bool IsDirectory;

    public string GetDisplay()
    {
        string[] sizeUnits = ["B", "KB", "MB", "GB"];
        var sizeUnitIndex = 0;
        var displaySize = Size;
        while (displaySize > 1024 && sizeUnitIndex < 3)
        {
            displaySize /= 1024;
            sizeUnitIndex += 1;
        }

        var result = new StringBuilder();

        result.Append(IsDirectory ? $"[cyan]{Name}[/]" : Name);

        if (displaySize > 0)
        {
            result.Append("[dim]");
            result.Append($" {Math.Round(displaySize, 2)}");
            result.Append($" {sizeUnits[sizeUnitIndex]}");
            result.Append("[/]");
        }

        return result.ToString();
    }
}