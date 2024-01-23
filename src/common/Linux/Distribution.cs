using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Linux.Enums;
using Spectre.Console;

namespace Linux;

public sealed class Distribution
{
    private readonly PackageManager _packageManager;
    private readonly Repository _repository;
    private readonly List<string> _installedPackages;

    public Distribution()
    {
        var osRelease = File.ReadAllText("/etc/os-release");
        switch (osRelease)
        {
            case var _ when osRelease.Contains("Arch"):
                _packageManager = PackageManager.Pacman;
                _repository = Repository.Arch;
                break;
            case var _ when osRelease.Contains("Alma"):
                _packageManager = PackageManager.Dnf;
                _repository = Repository.RedHat;
                break;
            case var _ when osRelease.Contains("CentOS"):
                _packageManager = PackageManager.Dnf;
                _repository = Repository.RedHat;
                break;
            case var _ when osRelease.Contains("Debian"):
                _packageManager = PackageManager.Apt;
                _repository = Repository.Debian;
                break;
            case var _ when osRelease.Contains("Silverblue"):
                _packageManager = PackageManager.RpmOsTree;
                _repository = Repository.Fedora;
                break;
            case var _ when osRelease.Contains("Fedora"):
                _packageManager = PackageManager.Dnf;
                _repository = Repository.Fedora;
                break;
            case var _ when osRelease.Contains("Mint"):
                _packageManager = PackageManager.Apt;
                _repository = Repository.Ubuntu;
                break;
            case var _ when osRelease.Contains("Ubuntu"):
                _packageManager = PackageManager.Apt;
                _repository = Repository.Ubuntu;
                break;
            default:
                throw new Exception("distribution not found");
        }

        _installedPackages = GetInstalledPackages();
    }

    private List<string> GetInstalledPackages()
    {
        var packages = new List<string>();

        var packageList = _packageManager switch
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

            var package = _packageManager switch
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

    public string GetPackageType() => _packageManager switch
    {
        PackageManager.Apt => "dpkg",
        PackageManager.Dnf => "rpm",
        PackageManager.Pacman => "pacman",
        _ => throw new NotImplementedException(),
    };

    public void RepositorySetup()
    {
        if (_packageManager == PackageManager.Dnf)
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
                        InstallPackage(
                            "https://download1.rpmfusion.org/free/fedora/rpmfusion-free-release-39.noarch.rpm");
                        break;
                    case Repository.RedHat:
                        InstallPackage("https://dl.fedoraproject.org/pub/epel/epel-release-latest-9.noarch.rpm");
                        InstallPackage("https://download1.rpmfusion.org/free/el/rpmfusion-free-release-9.noarch.rpm");
                        new Command("sudo dnf config-manager --set-enabled crb").Run();
                        break;
                }

                if (AnsiConsole.Confirm("Do you want to enable [red]Non-Free[/] EPEL/RPM Fusion Repositories?", false))
                {
                    switch (_repository)
                    {
                        case Repository.Fedora:
                            InstallPackage(
                                "https://download1.rpmfusion.org/nonfree/fedora/rpmfusion-nonfree-release-39.noarch.rpm");
                            break;
                        case Repository.RedHat:
                            InstallPackage(
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
        switch (_packageManager)
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
    }

    private void InstallPackage(string package)
    {
        if (_installedPackages.Contains(package)) return;
        _installedPackages.Add(package);

        switch (_packageManager)
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

    private void UnInstallPackage(string package)
    {
        if (!_installedPackages.Contains(package)) return;
        _installedPackages.Remove(package);

        switch (_packageManager)
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

    private void AutoRemove()
    {
        switch (_packageManager)
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
    }
}