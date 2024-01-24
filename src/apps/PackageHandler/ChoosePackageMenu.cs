using Linux;
using Spectre.Console;
using System.Collections.Generic;
using System.Text;
using Linux.Enums;

namespace PackageHandler;

internal sealed class ChoosePackageMenu
{
    private readonly Distribution _distribution;
    private readonly List<Package> _packages;

    public ChoosePackageMenu(Distribution distribution, List<Package> packages)
    {
        _distribution = distribution;
        _packages = packages;
        _packages.Add(new Package { Name = "Back" });
    }

    public void Run()
    {
        Package selectedPackage;
        do
        {
            selectedPackage = AnsiConsole.Prompt(
                new SelectionPrompt<Package>()
                    .Title("Choose a Package")
                    .PageSize(15)
                    .AddChoices(_packages)
                    .UseConverter(GetPackageDisplay)
            );

            if (selectedPackage.Name != "Back")
            {
                new ChooseInstallMethod(_distribution, selectedPackage).Run();
            }
        } while (selectedPackage.Name != "Back");
    }

    private string GetPackageDisplay(Package package)
    {
        var display = new StringBuilder();
        display.Append(package.Name);

        switch (_distribution.GetPackageInstallMethod(package))
        {
            case InstallMethod.Repository:
                display.Append(" [green](repository)[/]");
                break;
            case InstallMethod.Flatpak:
                display.Append(" [blue](flatpak)[/]");
                break;
            case InstallMethod.Snap:
                display.Append(" [purple](snap)[/]");
                break;
        }

        return display.ToString();
    }
}