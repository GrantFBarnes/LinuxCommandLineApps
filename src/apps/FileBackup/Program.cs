using Spectre.Console;

internal class Program
{
    private static string _homeDirectory = string.Empty;
    private static string _backupDirectory = string.Empty;
    private static List<string> _backupFolders = [];
    private static List<string> _encryptFolders = [];
    private static string _passphrase = string.Empty;

    private static void Main(string[] args)
    {
        _homeDirectory = Environment.GetEnvironmentVariable("HOME") ?? throw new Exception("HOME could not be determined");
        _backupDirectory = Path.Combine(_homeDirectory, "backups/home");
        Directory.CreateDirectory(_backupDirectory);

        _backupFolders = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select the [blue]folders[/] you want to [red]backup[/]:")
                .AddChoices(["Documents", "Music", "Pictures", "Videos"])
                .NotRequired()
        );

        if (_backupFolders.Count == 0)
        {
            Console.WriteLine("No selection made.");
            return;
        }

        if (AnsiConsole.Confirm("Do you want to [red]encrypt[/] backups?"))
        {
            _encryptFolders = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select the [blue]folders[/] you want to [red]encrypt[/]:")
                    .AddChoices(_backupFolders)
                    .NotRequired()
            );

            if (_encryptFolders.Count > 0)
            {
                _passphrase = AnsiConsole.Prompt(
                    new TextPrompt<string>("Encryption Passphrase:")
                        .PromptStyle("red")
                        .Secret()
                );

                var passphraseAgain = AnsiConsole.Prompt(
                    new TextPrompt<string>("Encryption Passphrase (Again):")
                        .PromptStyle("red")
                        .Secret()
                );

                if (_passphrase != passphraseAgain)
                {
                    AnsiConsole.MarkupLine("Entered values did [red]not[/] match.");
                    return;
                }
            }
        }

        foreach (var folder in _backupFolders)
        {
            var tarFile = Path.Combine(_backupDirectory, $"{folder}.tar.gz");
            var cryptFile = $"{tarFile}.gpg";

            File.Delete(tarFile);
            File.Delete(cryptFile);

            AnsiConsole.MarkupLine($"Compressing [blue]{folder}[/]...");

            new Command($"tar --exclude-vcs -cvzf {tarFile} {folder}")
                .WorkingDirectory(_homeDirectory)
                .Run();

            if (_encryptFolders.Contains(folder))
            {
                AnsiConsole.MarkupLine($"Encrypting [blue]{folder}[/]...");

                new Command($"gpg --batch -c --passphrase {_passphrase} {tarFile}")
                    .WorkingDirectory(_homeDirectory)
                    .Run();

                File.Delete(tarFile);
            }
        }
    }
}