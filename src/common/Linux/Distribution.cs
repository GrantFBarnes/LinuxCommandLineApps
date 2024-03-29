using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Linux.Enums;
using Linux.PackageTypes;
using Spectre.Console;

namespace Linux;

public sealed class Distribution
{
    public readonly PackageManager PackageManager;
    public readonly Repository Repository;
    public HashSet<DesktopEnvironment> InstalledDesktopEnvironments = [];
    private HashSet<string> _installedPackages = [];
    internal HashSet<string> InstalledFlatpaks = [];
    internal HashSet<string> InstalledSnaps = [];
    internal HashSet<OtherPackage> InstalledOther = [];

    public Distribution(bool waitForPackages = false)
    {
        var osRelease = File.ReadAllText("/etc/os-release");
        switch (osRelease)
        {
            case var _ when osRelease.Contains("Arch"):
                PackageManager = PackageManager.Pacman;
                Repository = Repository.Arch;
                break;
            case var _ when osRelease.Contains("Alma"):
                PackageManager = PackageManager.Dnf;
                Repository = Repository.RedHat;
                break;
            case var _ when osRelease.Contains("CentOS"):
                PackageManager = PackageManager.Dnf;
                Repository = Repository.RedHat;
                break;
            case var _ when osRelease.Contains("Debian"):
                PackageManager = PackageManager.Apt;
                Repository = Repository.Debian;
                break;
            case var _ when osRelease.Contains("Silverblue"):
                PackageManager = PackageManager.RpmOsTree;
                Repository = Repository.Fedora;
                break;
            case var _ when osRelease.Contains("Fedora"):
                PackageManager = PackageManager.Dnf;
                Repository = Repository.Fedora;
                break;
            case var _ when osRelease.Contains("Mint"):
                PackageManager = PackageManager.Apt;
                Repository = Repository.Ubuntu;
                break;
            case var _ when osRelease.Contains("Ubuntu"):
                PackageManager = PackageManager.Apt;
                Repository = Repository.Ubuntu;
                break;
            default:
                throw new Exception("distribution not found");
        }

        if (waitForPackages)
        {
            GetAllInstalled();
        }
        else
        {
            Task.Run(GetAllInstalled);
        }
    }

    private Task GetAllInstalled()
    {
        InstalledDesktopEnvironments = GetInstalledDesktopEnvironments();
        _installedPackages = GetInstalled();
        InstalledFlatpaks = [];
        InstalledSnaps = [];
        InstalledOther = [];

        var flatpakCommand = new Command("flatpak");
        if (flatpakCommand.Exists())
        {
            InstalledFlatpaks = Flatpak.GetInstalled();
        }

        var snapCommand = new Command("snap");
        if (snapCommand.Exists())
        {
            InstalledSnaps = Snap.GetInstalled();
        }

        InstalledOther = Other.GetInstalled();

        return Task.CompletedTask;
    }

    private HashSet<DesktopEnvironment> GetInstalledDesktopEnvironments()
    {
        var desktopEnvironments = new HashSet<DesktopEnvironment>();

        foreach (var de in Enum.GetValues(typeof(DesktopEnvironment)).Cast<DesktopEnvironment>())
        {
            switch (de)
            {
                case DesktopEnvironment.Gnome:
                    if (new Command("gnome-shell").Exists())
                    {
                        desktopEnvironments.Add(de);
                    }

                    break;
                case DesktopEnvironment.Kde:
                    if (new Command("plasmashell").Exists())
                    {
                        desktopEnvironments.Add(de);
                    }

                    break;
                default:
                    throw new Exception("desktop environment check not defined");
            }
        }

        return desktopEnvironments;
    }

    private HashSet<string> GetInstalled()
    {
        var packages = new HashSet<string>();

        var packageList = PackageManager switch
        {
            PackageManager.Apt => new Command("apt list --installed").GetOutput(),
            PackageManager.Dnf => new Command("dnf list installed").GetOutput(),
            PackageManager.Pacman => new Command("pacman -Q").GetOutput(),
            PackageManager.RpmOsTree => new Command("rpm -qa").GetOutput(),
            _ => throw new ArgumentOutOfRangeException()
        };

        foreach (var line in packageList.Split("\n"))
        {
            if (string.IsNullOrEmpty(line)) continue;

            var package = PackageManager switch
            {
                PackageManager.Apt => line.Split("/").First(),
                PackageManager.Dnf => string.Join(".", line.Split(null).First().Split(".").SkipLast(1)),
                PackageManager.Pacman => line.Split(null).First(),
                _ => string.Empty,
            };

            if (string.IsNullOrEmpty(package)) continue;

            packages.Add(package);
        }

        return packages;
    }

    public int GetPackageCount() => _installedPackages.Count;

    public int GetFlatpakCount() => InstalledFlatpaks.Count;

    public int GetSnapCount() => InstalledSnaps.Count;

    public int GetOtherCount() => InstalledOther.Count;

    public string GetPackageType() => PackageManager switch
    {
        PackageManager.Apt => "dpkg",
        PackageManager.Dnf => "rpm",
        PackageManager.Pacman => "pacman",
        PackageManager.RpmOsTree => "rpm",
        _ => throw new ArgumentOutOfRangeException()
    };

    public bool IsPackageAvailable(Package package)
    {
        return package.Repositories.ContainsKey(Repository)
               || package.Flatpak != null
               || package.Snap != null
               || package.Other != null;
    }

    public InstallMethod GetPackageInstallMethod(Package package)
    {
        if (package.Repositories.TryGetValue(Repository, out var packages))
        {
            if (packages.Any(x => _installedPackages.Contains(x)))
            {
                return InstallMethod.Repository;
            }
        }

        if (package.Flatpak != null && InstalledFlatpaks.Contains(package.Flatpak.Name))
        {
            return InstallMethod.Flatpak;
        }

        if (package.Snap != null && InstalledSnaps.Contains(package.Snap.Name))
        {
            return InstallMethod.Flatpak;
        }

        if (package.Other != null && InstalledOther.Contains(package.Other.Package))
        {
            return InstallMethod.Other;
        }

        return InstallMethod.None;
    }

    public List<InstallMethod> GetPackageInstallMethodOptions(Package package)
    {
        var options = new List<InstallMethod>();

        if (package.Repositories.ContainsKey(Repository))
        {
            options.Add(InstallMethod.Repository);
        }

        if (package.Flatpak != null)
        {
            options.Add(InstallMethod.Flatpak);
        }

        if (package.Snap != null)
        {
            options.Add(InstallMethod.Snap);
        }

        if (package.Other != null)
        {
            options.Add(InstallMethod.Other);
        }

        options.Add(InstallMethod.Uninstall);
        options.Add(InstallMethod.None);

        return options;
    }

    public void RepositorySetup()
    {
        if (PackageManager == PackageManager.Dnf)
        {
            const string dnfConfFile = "/etc/dnf/dnf.conf";
            if (!File.Exists(dnfConfFile) || !File.ReadAllText(dnfConfFile).Contains("max_parallel_downloads"))
            {
                new Command($"echo \"max_parallel_downloads=10\"")
                    .HideOutput(true)
                    .PipeInto($"sudo tee --append {dnfConfFile}");
            }

            if (AnsiConsole.Confirm("Do you want to enable EPEL/RPM Fusion Repositories?", false))
            {
                switch (Repository)
                {
                    case Repository.Fedora:
                        Install("https://download1.rpmfusion.org/free/fedora/rpmfusion-free-release-39.noarch.rpm");
                        break;
                    case Repository.RedHat:
                        Install("https://dl.fedoraproject.org/pub/epel/epel-release-latest-9.noarch.rpm");
                        Install("https://download1.rpmfusion.org/free/el/rpmfusion-free-release-9.noarch.rpm");
                        new Command("sudo dnf config-manager --set-enabled crb").Run();
                        break;
                }

                if (AnsiConsole.Confirm("Do you want to enable [red]Non-Free[/] EPEL/RPM Fusion Repositories?", false))
                {
                    switch (Repository)
                    {
                        case Repository.Fedora:
                            Install(
                                "https://download1.rpmfusion.org/nonfree/fedora/rpmfusion-nonfree-release-39.noarch.rpm");
                            break;
                        case Repository.RedHat:
                            Install(
                                "https://download1.rpmfusion.org/nonfree/el/rpmfusion-nonfree-release-9.noarch.rpm");
                            break;
                    }
                }

                Update();
            }
        }
    }

    public void Update()
    {
        switch (PackageManager)
        {
            case PackageManager.Apt:
                new Command("sudo apt update").Run();
                new Command("sudo apt upgrade -Vy").Run();
                break;
            case PackageManager.Dnf:
                new Command("sudo dnf upgrade --refresh -y").Run();
                break;
            case PackageManager.Pacman:
                new Command("sudo pacman -Syu --noconfirm").Run();
                break;
            case PackageManager.RpmOsTree:
                new Command("rpm-ostree upgrade").Run();
                break;
        }

        var flatpakCommand = new Command("flatpak");
        if (flatpakCommand.Exists())
        {
            Flatpak.Update();
        }

        var snapCommand = new Command("snap");
        if (snapCommand.Exists())
        {
            Snap.Update();
        }

        Other.Update();
    }

    public void InstallPackage(Package package)
    {
        if (!package.Repositories.TryGetValue(Repository, out var packageList)) return;
        foreach (var pkg in packageList)
        {
            Install(pkg);
        }
    }

    public void Install(string package)
    {
        if (_installedPackages.Contains(package)) return;
        _installedPackages.Add(package);

        switch (PackageManager)
        {
            case PackageManager.Apt:
                new Command($"sudo apt install {package} -Vy").Run();
                break;
            case PackageManager.Dnf:
                new Command($"sudo dnf install {package} -y").Run();
                break;
            case PackageManager.Pacman:
                new Command($"sudo pacman -S {package} --noconfirm --needed").Run();
                break;
            case PackageManager.RpmOsTree:
                new Command($"sudo rpm-ostree install {package} -y").Run();
                break;
        }
    }

    public void UnInstallPackage(Package package)
    {
        if (!package.Repositories.TryGetValue(Repository, out var packageList)) return;
        foreach (var pkg in packageList)
        {
            UnInstall(pkg);
        }
    }

    private void UnInstall(string package)
    {
        if (!_installedPackages.Contains(package)) return;
        _installedPackages.Remove(package);

        switch (PackageManager)
        {
            case PackageManager.Apt:
                new Command($"sudo apt remove {package} -Vy").Run();
                break;
            case PackageManager.Dnf:
                new Command($"sudo dnf remove {package} -y").Run();
                break;
            case PackageManager.Pacman:
                new Command($"sudo pacman -Runs {package} --noconfirm").Run();
                break;
            case PackageManager.RpmOsTree:
                new Command($"sudo rpm-ostree uninstall {package} -y").Run();
                break;
        }
    }

    public void AutoRemove()
    {
        switch (PackageManager)
        {
            case PackageManager.Apt:
                new Command("sudo apt autoremove -Vy").Run();
                break;
            case PackageManager.Dnf:
                new Command("sudo dnf autoremove -y").Run();
                break;
            case PackageManager.Pacman:
                new Command("pacman -Qdtq").PipeInto("sudo pacman -Rs -");
                break;
        }

        var flatpakCommand = new Command("flatpak");
        if (flatpakCommand.Exists())
        {
            Flatpak.AutoRemove();
        }
    }
}