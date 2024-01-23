using System;
using Linux;
using System.Collections.Generic;
using Linux.Enums;
using PackageHandler.Enums;
using Spectre.Console;

var packages = GetPackages(PackageCategory.Server);

var selectedPackage = AnsiConsole.Prompt(
    new SelectionPrompt<Package>()
        .Title("Select a Package")
        .PageSize(15)
        .AddChoices(packages)
        .UseConverter(x => x.Name)
);

Console.WriteLine($"You chose {selectedPackage.Name}");

return;

List<Package> GetPackages(PackageCategory category)
{
    return category switch
    {
        PackageCategory.Server =>
        [
            new Package()
            {
                Name = "flatpak",
                DesktopEnvironment = null,
                Repositories = new Dictionary<Repository, List<string>>()
                {
                    { Repository.Arch, ["flatpak"] },
                    { Repository.Debian, ["flatpak"] },
                    { Repository.Fedora, ["flatpak"] },
                    { Repository.RedHat, ["flatpak"] },
                    { Repository.Ubuntu, ["flatpak"] },
                },
                Flatpak = null,
                Snap = null,
                PreInstall = null,
                PostInstall = null,
            }
        ],
        PackageCategory.Desktop => [],
        PackageCategory.Applications => [],
        PackageCategory.Browsers => [],
        PackageCategory.Communication => [],
        PackageCategory.Games => [],
        PackageCategory.MultiMedia => [],
        PackageCategory.Editors => [],
        PackageCategory.Software => [],
        PackageCategory.Utilities => [],
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
    };
}