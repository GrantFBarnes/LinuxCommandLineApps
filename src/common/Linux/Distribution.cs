using System;
using System.IO;
using Linux.Enums;

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
}