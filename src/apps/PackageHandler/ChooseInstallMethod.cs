using Linux.Enums;
using Linux;
using Spectre.Console;
using System.Collections.Generic;

namespace PackageHandler;

internal sealed class ChooseInstallMethod(Distribution distribution, Package package)
{
    private InstallMethod _installedAsMethod = distribution.GetPackageInstallMethod(package);
    private readonly List<InstallMethod> _installMethodOptions = distribution.GetPackageInstallMethodOptions(package);

    public void Run()
    {
        while (true)
        {
            _installedAsMethod = distribution.GetPackageInstallMethod(package);
            var selectedMethod = AnsiConsole.Prompt(
                new SelectionPrompt<InstallMethod>()
                    .Title("Choose an Install Method")
                    .PageSize(15)
                    .AddChoices(_installMethodOptions)
                    .UseConverter(GetInstallMethodDisplay)
            );

            if (selectedMethod == InstallMethod.None)
            {
                break;
            }

            if (selectedMethod == InstallMethod.Repository)
            {
                distribution.InstallPackage(package);
            }
            else
            {
                distribution.UnInstallPackage(package);
            }

            if (selectedMethod == InstallMethod.Flatpak)
            {
                new ChooseFlatpakRemote(distribution, package.Flatpak).Run();
            }
            else
            {
                package.Flatpak?.UnInstall(distribution);
            }

            if (selectedMethod == InstallMethod.Snap)
            {
                package.Snap?.Install(distribution);
            }
            else
            {
                package.Snap?.UnInstall(distribution);
            }
        }
    }

    private string GetInstallMethodDisplay(InstallMethod method)
    {
        var display = string.Empty;
        switch (method)
        {
            case InstallMethod.Repository:
                display = "[green]Repository[/]";
                break;
            case InstallMethod.Flatpak:
                display = "[blue]Flatpak[/]";
                break;
            case InstallMethod.Snap:
                display = "[purple]Snap[/]";
                if (package.Snap != null)
                {
                    if (package.Snap.IsOfficial)
                    {
                        display += " - Official";
                    }

                    if (package.Snap.IsClassic)
                    {
                        display += " [darkgoldenrod](classic)[/]";
                    }
                }

                break;
            case InstallMethod.Uninstall:
                display = "[red]Uninstall[/]";
                break;
            case InstallMethod.None:
                display = "Back";
                break;
        }

        if (method != InstallMethod.None && method == _installedAsMethod)
        {
            display += " [cyan](installed)[/]";
        }

        return display;
    }
}