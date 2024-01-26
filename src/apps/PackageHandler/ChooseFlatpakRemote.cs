using Linux.Enums;
using Linux;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;

namespace PackageHandler;

internal sealed class ChooseFlatpakRemote(Distribution distribution, Flatpak flatpak)
{
    private readonly List<FlatpakRemote> _remotes = flatpak.Remotes;

    public void Run()
    {
        if (_remotes.Count == 1)
        {
            flatpak.Install(flatpak.Remotes.First(), distribution);
            return;
        }

        while (true)
        {
            var selectedRemote = AnsiConsole.Prompt(
                new SelectionPrompt<FlatpakRemote>()
                    .Title("Choose a Flatpak Remote")
                    .PageSize(15)
                    .AddChoices(_remotes)
                    .UseConverter(x => x.ToString())
            );

            if (selectedRemote == FlatpakRemote.None)
            {
                break;
            }

            flatpak.Install(selectedRemote, distribution);
        }
    }
}