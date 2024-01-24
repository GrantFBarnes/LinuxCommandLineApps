using Linux.Enums;
using Linux;
using Spectre.Console;
using System.Collections.Generic;

namespace PackageHandler;

internal sealed class ChooseInstallMethod(Distribution distribution, Package package)
{
    private readonly InstallMethod _installedAsMethod = distribution.GetPackageInstallMethod(package);
    private readonly List<InstallMethod> _installMethodOptions = distribution.GetPackageInstallMethodOptions(package);

    public void Run()
    {
        InstallMethod selectedMethod;
        do
        {
            selectedMethod = AnsiConsole.Prompt(
                new SelectionPrompt<InstallMethod>()
                    .Title("Choose an Install Method")
                    .PageSize(15)
                    .AddChoices(_installMethodOptions)
                    .UseConverter(GetInstallMethodDisplay)
            );
        } while (selectedMethod != InstallMethod.None);
    }

    private string GetInstallMethodDisplay(InstallMethod method)
    {
        var display = method switch
        {
            InstallMethod.Repository => "Repository",
            InstallMethod.Flatpak => "Flatpak",
            InstallMethod.Snap => "Snap",
            InstallMethod.None => "Back",
            _ => "",
        };

        if (method != InstallMethod.None && method == _installedAsMethod)
        {
            display += " [red](installed)[/]";
        }

        return display;
    }
}