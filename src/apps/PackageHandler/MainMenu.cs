using Linux;
using Spectre.Console;
using System.Collections.Generic;
using System;

namespace PackageHandler;

internal sealed class MainMenu(Distribution distribution)
{
    private readonly Dictionary<string, Action> _actions = new()
    {
        { "Repository Setup", distribution.RepositorySetup },
        { "Update Packages", distribution.Update },
        { "Install Packages", new ChoosePackageCategoryMenu(distribution).Run },
        { "Auto Remove Packages", distribution.AutoRemove },
        { "Exit", () => { } },
    };

    public void Run()
    {
        var selectedAction = string.Empty;
        while (selectedAction != "Exit")
        {
            selectedAction = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Main Menu")
                    .PageSize(15)
                    .AddChoices(_actions.Keys)
            );
            _actions[selectedAction]();
        }
    }
}