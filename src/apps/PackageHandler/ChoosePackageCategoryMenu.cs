using Linux.Enums;
using Linux;
using PackageHandler.Enums;
using Spectre.Console;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PackageHandler;

internal sealed class ChoosePackageCategoryMenu(Distribution distribution)
{
    public void Run()
    {
        while (true)
        {
            var selectedCategory = AnsiConsole.Prompt(
                new SelectionPrompt<PackageCategory>()
                    .Title("Choose a Package Category")
                    .PageSize(15)
                    .AddChoices(Enum.GetValues(typeof(PackageCategory)).Cast<PackageCategory>())
                    .UseConverter(x => x.ToString())
            );

            if (selectedCategory == PackageCategory.Back)
            {
                break;
            }

            new ChoosePackageMenu(distribution, GetPackages(selectedCategory)).Run();
        }
    }

    private static List<Package> GetPackages(PackageCategory category)
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
            PackageCategory.Back => [],
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
    }
}