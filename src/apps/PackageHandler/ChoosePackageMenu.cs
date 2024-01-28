using Linux.Enums;
using Linux;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PackageHandler;

internal sealed class ChoosePackageMenu
{
    private readonly Distribution _distribution;
    private readonly List<Package> _packages;

    public ChoosePackageMenu(Distribution distribution, IEnumerable<Package> packages)
    {
        _distribution = distribution;
        _packages = packages.Where(distribution.IsPackageAvailable).ToList();
        _packages.Add(new Package { Name = "Back" });
    }

    public void Run()
    {
        while (true)
        {
            var selectedPackage = AnsiConsole.Prompt(
                new SelectionPrompt<Package>()
                    .Title("Choose a Package")
                    .PageSize(15)
                    .AddChoices(_packages)
                    .UseConverter(GetPackageDisplay)
            );

            if (selectedPackage.Name == "Back")
            {
                break;
            }

            new ChooseInstallMethodMenu(_distribution, selectedPackage).Run();
        }
    }

    private string GetPackageDisplay(Package package)
    {
        if (package.Name == "Back") return "Back";

        var display = new StringBuilder();

        if (package.DesktopEnvironment != null &&
            !_distribution.InstalledDesktopEnvironments.Contains((DesktopEnvironment)package.DesktopEnvironment))
        {
            display.Append($"[orange4_1]{package.Name}[/]");
        }
        else
        {
            display.Append(package.Name);
        }


        switch (_distribution.GetPackageInstallMethod(package))
        {
            case InstallMethod.Repository:
                display.Append(" [green](repository installed)[/]");
                break;
            case InstallMethod.Flatpak:
                display.Append(" [blue](flatpak installed)[/]");
                break;
            case InstallMethod.Snap:
                display.Append(" [purple](snap installed)[/]");
                break;
            case InstallMethod.Other:
                display.Append(" [darkgoldenrod](other installed)[/]");
                break;
            default:
                display.Append(" [red](uninstalled)[/]");
                break;
        }

        return display.ToString();
    }
}