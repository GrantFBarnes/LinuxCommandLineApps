// Overview

using System.Security.Cryptography;
using System.Text;

Console.WriteLine($"    User: {GetUser()}");
Console.WriteLine($"Hostname: {GetHostname()}");
Console.WriteLine($"      OS: {GetOS()}");
Console.WriteLine($"  Kernel: {GetKernel()}");
Console.WriteLine($"     CPU: {GetCPU()}");
Console.WriteLine($"   Speed: {GetSpeed()}");
Console.WriteLine($"  Memory: {GetMemory()}");
Console.WriteLine($"  Uptime: {GetUptime()}");
Console.WriteLine($"Packages: {GetPackages()}");

const string unknown = "(Unknown)";

static string GetUser()
{
    return Environment.GetEnvironmentVariable("HOME")?.Split("/").Last() ?? unknown;
}

static string GetHostname()
{
    var hostname = File.ReadAllText("/etc/hostname").Trim();
    if (string.IsNullOrEmpty(hostname))
    {
        return unknown;
    }
    return hostname;
}

static string GetOS()
{
    string[] files = ["/etc/lsb-release", "/usr/lib/os-release", "/etc/os-release"];
    foreach (var file in files)
    {
        try
        {
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                if (line.StartsWith("PRETTY_NAME") || line.StartsWith("DISTRIB_DESCRIPTION"))
                {
                    var split = line.Split("\"");
                    if (split.Length > 1)
                    {
                        return split[1].Trim();
                    }
                }
            }
        }
        catch { }
    }

    return unknown;
}

static string GetKernel()
{
    return new Command("uname -r").GetOutput().Trim();
}

static string GetCPU()
{
    var lines = File.ReadAllLines("/proc/cpuinfo");
    foreach (var line in lines)
    {
        if (line.StartsWith("model name"))
        {
            return line.Split(": ").Last().Trim();
        }
    }
    return unknown;
}

static string GetSpeed()
{
    var processorSpeeds = new List<float>();
    var currentSpeed = 0.0;
    var maxSpeed = 0.0;

    var cpuInfoLines = File.ReadAllLines("/proc/cpuinfo");
    foreach (var line in cpuInfoLines)
    {
        if (line.StartsWith("cpu MHz"))
        {
            var speedString = line.Split(": ").Last().Trim();
            if (float.TryParse(speedString, out float speedFloat))
            {
                processorSpeeds.Add(speedFloat);
            }
        }
    }

    if (processorSpeeds.Count > 0)
    {
        var sum = processorSpeeds.Sum();
        currentSpeed = sum / processorSpeeds.Count / 1000;
    }

    string[] files = [
        "/sys/devices/system/cpu/cpu0/cpufreq/bios_limit",
        "/sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_max_freq",
        "/sys/devices/system/cpu/cpu0/cpufreq/scaling_max_freq",
    ];
    foreach (var file in files)
    {
        try
        {
            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                if (char.IsNumber(line.First()))
                {
                    maxSpeed = float.Parse(line) / 1000 / 1000;
                }
            }
        }
        catch { }
    }

    var result = new StringBuilder();
    if (currentSpeed > 0)
    {
        result.Append($"{Math.Round(currentSpeed, 2)} GHz");
    }

    if (maxSpeed > 0)
    {
        if (result.Length > 0)
        {
            result.Append(" / ");
        }
        result.Append($"{Math.Round(maxSpeed, 2)} GHz");

        var percentage = currentSpeed / maxSpeed * 100;
        if (percentage > 0)
        {
            result.Append($" ({Math.Round(percentage, 2)}%)");
        }
    }

    return result.ToString();
}

static string GetMemory()
{
    float totalMemory = 0;
    float availableMemory = 0;

    var memInfoLines = File.ReadAllLines("/proc/meminfo");
    foreach (var line in memInfoLines)
    {
        if (line.StartsWith("MemTotal"))
        {
            var memString = line.Split(null).SkipLast(1).Last() ?? string.Empty;
            _ = float.TryParse(memString, out totalMemory);
        }
        else if (line.StartsWith("MemAvailable"))
        {
            var memString = line.Split(null).SkipLast(1).Last() ?? string.Empty;
            _ = float.TryParse(memString, out availableMemory);
        }
    }

    if (totalMemory > 0)
    {
        var usedMemory = totalMemory - availableMemory;
        var percentage = usedMemory / totalMemory * 100;
        return $"{Math.Round(usedMemory / 1024 / 1024, 2)}/{Math.Round(totalMemory / 1024 / 1024, 2)} GB ({Math.Round(percentage, 2)}%)";
    }

    return unknown;
}

static string GetUptime()
{
    return new Command("uptime -p").GetOutput().Trim();
}

static string GetPackages()
{
    var result = new List<string>();

    var distribution = new Distribution();
    result.Add($"{distribution.GetPackageCount()} ({distribution.GetPackageType()})");

    var flatpakCommand = new Command("flatpak list");
    if (flatpakCommand.Exists())
    {
        result.Add($"{flatpakCommand.GetOutput().Split("\n").Length} (flatpak)");
    }

    var snapCommand = new Command("snap list");
    if (snapCommand.Exists())
    {
        result.Add($"{snapCommand.GetOutput().Split("\n").Length} (snap)");
    }

    return string.Join(", ", result);
}