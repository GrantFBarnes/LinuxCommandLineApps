using Linux.Enums;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Linux;

public sealed class Snap(string name, bool isOfficial = false, bool isClassic = false, string? channel = null)
{
    public readonly string Name = name;
    public readonly bool IsOfficial = isOfficial;
    public readonly bool IsClassic = isClassic;

    public static List<string> GetInstalled()
    {
        var packages = new List<string>();

        var packageList = new Command("snap list").GetOutput();

        foreach (var line in packageList.Split("\n"))
        {
            if (string.IsNullOrEmpty(line)) continue;
            var package = line.Split(null).First();
            if (string.IsNullOrEmpty(package)) continue;
            packages.Add(package);
        }

        return packages;
    }

    public static void Setup(Distribution distribution)
    {
        if (distribution.PackageManager == PackageManager.Dnf)
        {
            new Command("sudo systemctl enable --now snapd.socket").Run();
            new Command("sudo ln -s /var/lib/snapd/snap /snap").Run();
        }
    }

    public static void Update()
    {
        new Command("sudo snap refresh").Run();
    }

    public void Install(Distribution distribution)
    {
        if (distribution.InstalledSnaps.Contains(Name)) return;
        distribution.InstalledSnaps.Add(Name);

        distribution.Install("snapd");
        Setup(distribution);

        var installCommand = new StringBuilder();
        installCommand.Append($"sudo snap install {Name}");

        if (IsClassic)
        {
            installCommand.Append(" --classic");
        }

        if (string.IsNullOrEmpty(channel))
        {
            installCommand.Append($" --channel {channel}");
        }

        new Command(installCommand.ToString()).Run();
    }

    public void UnInstall(Distribution distribution)
    {
        if (!distribution.InstalledSnaps.Contains(Name)) return;
        distribution.InstalledSnaps.Remove(Name);

        new Command($"sudo snap remove {Name}").Run();
    }
}