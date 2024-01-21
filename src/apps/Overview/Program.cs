// Overview

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
    throw new NotImplementedException();
}

static string GetSpeed()
{
    throw new NotImplementedException();
}

static string GetMemory()
{
    throw new NotImplementedException();
}

static string GetUptime()
{
    throw new NotImplementedException();
}

static string GetPackages()
{
    throw new NotImplementedException();
}