using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Linux.Enums;
using Spectre.Console;

namespace Linux;

public sealed class Distribution
{
    internal readonly PackageManager PackageManager;
    private readonly Repository _repository;
    private readonly List<string> _installedPackages;
    internal readonly List<string> InstalledFlatpaks;
    internal readonly List<string> InstalledSnaps;

    public Distribution()
    {
        var osRelease = File.ReadAllText("/etc/os-release");
        switch (osRelease)
        {
            case var _ when osRelease.Contains("Arch"):
                PackageManager = PackageManager.Pacman;
                _repository = Repository.Arch;
                break;
            case var _ when osRelease.Contains("Alma"):
                PackageManager = PackageManager.Dnf;
                _repository = Repository.RedHat;
                break;
            case var _ when osRelease.Contains("CentOS"):
                PackageManager = PackageManager.Dnf;
                _repository = Repository.RedHat;
                break;
            case var _ when osRelease.Contains("Debian"):
                PackageManager = PackageManager.Apt;
                _repository = Repository.Debian;
                break;
            case var _ when osRelease.Contains("Silverblue"):
                PackageManager = PackageManager.RpmOsTree;
                _repository = Repository.Fedora;
                break;
            case var _ when osRelease.Contains("Fedora"):
                PackageManager = PackageManager.Dnf;
                _repository = Repository.Fedora;
                break;
            case var _ when osRelease.Contains("Mint"):
                PackageManager = PackageManager.Apt;
                _repository = Repository.Ubuntu;
                break;
            case var _ when osRelease.Contains("Ubuntu"):
                PackageManager = PackageManager.Apt;
                _repository = Repository.Ubuntu;
                break;
            default:
                throw new Exception("distribution not found");
        }

        _installedPackages = GetInstalled();
        InstalledFlatpaks = [];
        InstalledSnaps = [];

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
    }

    private List<string> GetInstalled()
    {
        var packages = new List<string>();

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

    public string GetPackageType() => PackageManager switch
    {
        PackageManager.Apt => "dpkg",
        PackageManager.Dnf => "rpm",
        PackageManager.Pacman => "pacman",
        _ => throw new NotImplementedException(),
    };

    public InstallMethod GetPackageInstallMethod(Package package)
    {
        if (package.Repositories.TryGetValue(_repository, out var packages))
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

        return InstallMethod.None;
    }

    public List<InstallMethod> GetPackageInstallMethodOptions(Package package)
    {
        var options = new List<InstallMethod>();

        if (package.Repositories.ContainsKey(_repository))
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
                switch (_repository)
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
                    switch (_repository)
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
    }

    public void InstallPackage(Package package)
    {
        if (!package.Repositories.TryGetValue(_repository, out var packageList)) return;
        foreach (var pkg in packageList)
        {
            Install(pkg);
        }
    }

    internal void Install(string package)
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
        if (!package.Repositories.TryGetValue(_repository, out var packageList)) return;
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
                new Command($"sudo pacman -Rsun {package} --noconfirm").Run();
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