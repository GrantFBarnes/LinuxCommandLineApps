using Linux;
using Spectre.Console;
using System.Collections.Generic;

namespace PackageHandler;

internal sealed class ChoosePackageMenu
{
    private readonly List<Package> _packages;

    public ChoosePackageMenu(Distribution distribution, List<Package> packages)
    {
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
                    .UseConverter(x => x.Name)
            );
        } while (selectedPackage.Name != "Back");
    }
}