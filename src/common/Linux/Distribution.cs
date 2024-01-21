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
            case string x when x.Contains("Arch"):
                _name = DistributionName.Arch;
                _repository = Repository.Arch;
                _packageManager = PackageManager.PACMAN;
                break;
            case string x when x.Contains("Alma"):
                _name = DistributionName.Alma;
                _repository = Repository.RedHat;
                _packageManager = PackageManager.DNF;
                break;
            case string x when x.Contains("CentOS"):
                _name = DistributionName.CentOS;
                _repository = Repository.RedHat;
                _packageManager = PackageManager.DNF;
                break;
            case string x when x.Contains("Debian"):
                _name = DistributionName.Debian;
                _repository = Repository.Debian;
                _packageManager = PackageManager.APT;
                break;
            case string x when x.Contains("Silverblue"):
                _name = DistributionName.SilverBlue;
                _repository = Repository.Fedora;
                _packageManager = PackageManager.RPMOSTree;
                break;
            case string x when x.Contains("Fedora"):
                _name = DistributionName.Fedora;
                _repository = Repository.Fedora;
                _packageManager = PackageManager.DNF;
                break;
            case string x when x.Contains("Mint"):
                _name = DistributionName.Mint;
                _repository = Repository.Ubuntu;
                _packageManager = PackageManager.APT;
                break;
            case string x when x.Contains("Ubuntu"):
                _name = DistributionName.Ubuntu;
                _repository = Repository.Ubuntu;
                _packageManager = PackageManager.APT;
                break;
            default:
                throw new Exception("distribution not found");
        }
    }
}