using System;
using System.IO;
using Linux.Enums;
using Spectre.Console;

namespace Linux;

public sealed class Distribution
{
    private readonly DistributionName _name;
    private readonly PackageManager _packageManager;
    private readonly Repository _repository;

    public Distribution()
    {
        var osRelease = File.ReadAllText("/etc/os-release");
        switch (osRelease)
        {
            case var _ when osRelease.Contains("Arch"):
                _name = DistributionName.Arch;
                _repository = Repository.Arch;
                _packageManager = PackageManager.Pacman;
                break;
            case var _ when osRelease.Contains("Alma"):
                _name = DistributionName.Alma;
                _repository = Repository.RedHat;
                _packageManager = PackageManager.Dnf;
                break;
            case var _ when osRelease.Contains("CentOS"):
                _name = DistributionName.CentOs;
                _repository = Repository.RedHat;
                _packageManager = PackageManager.Dnf;
                break;
            case var _ when osRelease.Contains("Debian"):
                _name = DistributionName.Debian;
                _repository = Repository.Debian;
                _packageManager = PackageManager.Apt;
                break;
            case var _ when osRelease.Contains("Silverblue"):
                _name = DistributionName.SilverBlue;
                _repository = Repository.Fedora;
                _packageManager = PackageManager.RpmOsTree;
                break;
            case var _ when osRelease.Contains("Fedora"):
                _name = DistributionName.Fedora;
                _repository = Repository.Fedora;
                _packageManager = PackageManager.Dnf;
                break;
            case var _ when osRelease.Contains("Mint"):
                _name = DistributionName.Mint;
                _repository = Repository.Ubuntu;
                _packageManager = PackageManager.Apt;
                break;
            case var _ when osRelease.Contains("Ubuntu"):
                _name = DistributionName.Ubuntu;
                _repository = Repository.Ubuntu;
                _packageManager = PackageManager.Apt;
                break;
            default:
                throw new Exception("distribution not found");
        }
    }

    public int GetPackageCount() => _packageManager switch
    {
        PackageManager.Apt => new Command("dpkg --list").GetOutput().Split("\n").Length,
        PackageManager.Dnf => new Command("rpm -qa").GetOutput().Split("\n").Length,
        PackageManager.Pacman => new Command("pacman -Q").GetOutput().Split("\n").Length,
        _ => throw new NotImplementedException(),
    };

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
                new Command($"echo \"max_parallel_downloads=10\"").PipeInto($"sudo tee --append {dnfConfFile}");
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
}