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
                    Name = "dotnet - C# SDK 8.0 LTS",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["dotnet-sdk-8.0"] },
                        { Repository.Debian, ["dotnet-sdk-8.0"] },
                        { Repository.Fedora, ["dotnet-sdk-8.0"] },
                        { Repository.RedHat, ["dotnet-sdk-8.0"] },
                        { Repository.Ubuntu, ["dotnet-sdk-8.0"] },
                    },
                    Flatpak = null,
                    Snap = new Snap("dotnet-sdk", true, true, "8.0/stable"),
                    PreInstall = null,
                    PostInstall = null,
                },
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
                },
                new Package()
                {
                    Name = "htop",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["htop"] },
                        { Repository.Debian, ["htop"] },
                        { Repository.Fedora, ["htop"] },
                        { Repository.RedHat, ["htop"] },
                        { Repository.Ubuntu, ["htop"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
            ],
            PackageCategory.Desktop => [],
            PackageCategory.Applications => [],
            PackageCategory.Browsers => [],
            PackageCategory.Communication => [],
            PackageCategory.Games => [],
            PackageCategory.MultiMedia => [],
            PackageCategory.Editors =>
            [
                new Package()
                {
                    Name = "Gnome Builder",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-builder"] },
                        { Repository.Debian, ["gnome-builder"] },
                        { Repository.Fedora, ["gnome-builder"] },
                        { Repository.Ubuntu, ["gnome-builder"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Builder", [FlatpakRemote.FlatHub]),
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "VS Code",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["code"] },
                        { Repository.Debian, ["code"] },
                        { Repository.Fedora, ["code"] },
                        { Repository.RedHat, ["code"] },
                    },
                    Flatpak = new Flatpak("com.visualstudio.code", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("code", true, true),
                    PreInstall = null,
                    PostInstall = null,
                },
            ],
            PackageCategory.Software => [],
            PackageCategory.Utilities => [],
            PackageCategory.Back => [],
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
    }
}