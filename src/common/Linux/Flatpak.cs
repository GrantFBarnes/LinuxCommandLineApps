using Linux.Enums;
using System.Linq;
using System.Collections.Generic;

namespace Linux;

public sealed class Flatpak(string name, List<FlatpakRemote> remotes)
{
    public static List<string> GetInstalled()
    {
        var packages = new List<string>();

        var packageList = new Command("flatpak list --app").GetOutput();

        foreach (var line in packageList.Split("\n"))
        {
            if (string.IsNullOrEmpty(line)) continue;

            var package = string.Empty;
            var split = line.Split("\t");
            if (split.Length > 1)
            {
                package = split.Skip(1).First();
            }

            if (string.IsNullOrEmpty(package)) continue;

            packages.Add(package);
        }

        return packages;
    }

    private static void Setup(Distribution distribution)
    {
        new Command("flatpak remote-add --if-not-exists flathub https://flathub.org/repo/flathub.flatpakrepo")
            .HideOutput(true)
            .Run();

        if (distribution.PackageManager == PackageManager.Dnf)
        {
            new Command("flatpak remote-add --if-not-exists fedora oci+https://registry.fedoraproject.org")
                .HideOutput(true)
                .Run();
        }
    }

    public static void Update()
    {
        new Command("flatpak update -y").Run();
    }

    public void Install(FlatpakRemote remote, Distribution distribution)
    {
        distribution.InstallPackage("flatpak");
        Setup(distribution);

        new Command($"flatpak install {remote} {name} -y").Run();
    }

    public void UnInstall()
    {
        new Command($"flatpak remove {name} -y").Run();
    }

    public static void AutoRemove()
    {
        new Command("flatpak remove --unused -y").Run();
    }
}