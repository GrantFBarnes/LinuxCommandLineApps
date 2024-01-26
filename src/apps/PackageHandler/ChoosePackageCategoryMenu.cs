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
                    Name = "Cockpit - Web Interface",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["cockpit"] },
                        { Repository.Debian, ["cockpit"] },
                        { Repository.Fedora, ["cockpit"] },
                        { Repository.RedHat, ["cockpit"] },
                        { Repository.Ubuntu, ["cockpit"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "cURL - Client URL",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["curl"] },
                        { Repository.Debian, ["curl"] },
                        { Repository.Fedora, ["curl"] },
                        { Repository.RedHat, ["curl"] },
                        { Repository.Ubuntu, ["curl"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "dotnet - C# runtime 8.0 LTS",
                    DesktopEnvironment = null,

                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["dotnet-runtime-8.0"] },
                        { Repository.Debian, ["dotnet-runtime-8.0"] },
                        { Repository.Fedora, ["dotnet-runtime-8.0"] },
                        { Repository.RedHat, ["dotnet-runtime-8.0"] },
                        { Repository.Ubuntu, ["dotnet-runtime-8.0"] },
                    },
                    Flatpak = null,
                    Snap = new Snap("dotnet-runtime-80", true, false),
                    PreInstall = null,
                    PostInstall = null,
                },
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
                    Name = "Flutter",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>(),
                    Flatpak = null,
                    Snap = new Snap("flutter", true, true),
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "git - Version Control",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["git"] },
                        { Repository.Debian, ["git"] },
                        { Repository.Fedora, ["git"] },
                        { Repository.RedHat, ["git"] },
                        { Repository.Ubuntu, ["git"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "Go Language",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["go", "gopls"] },
                        { Repository.Debian, ["golang", "gopls"] },
                        { Repository.Fedora, ["golang", "golang-x-tools-gopls"] },
                        { Repository.RedHat, ["golang"] },
                        { Repository.Ubuntu, ["golang", "gopls"] },
                    },
                    Flatpak = null,
                    Snap = new Snap("go", true, true),
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "htop - Process Reviewer",
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
                new Package()
                {
                    Name = "MariaDB - Database",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["mariadb"] },
                        { Repository.Debian, ["mariadb-server"] },
                        { Repository.Fedora, ["mariadb-server"] },
                        { Repository.RedHat, ["mariadb-server"] },
                        { Repository.Ubuntu, ["mariadb-server"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "neovim - Text Editor",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        {
                            Repository.Arch,
                            ["neovim", "vim", "vim-airline", "vim-ale", "vim-ctrlp", "vim-gitgutter", "vim-nerdtree"]
                        },
                        {
                            Repository.Debian,
                            ["neovim", "vim", "vim-airline", "vim-ale", "vim-ctrlp", "vim-gitgutter"]
                        },
                        {
                            Repository.Fedora,
                            [
                                "neovim", "vim-enhanced", "vim-airline", "vim-ale", "vim-ctrlp", "vim-gitgutter",
                                "vim-nerdtree"
                            ]
                        },
                        {
                            Repository.Ubuntu,
                            ["neovim", "vim", "vim-airline", "vim-ale", "vim-ctrlp", "vim-gitgutter"]
                        },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "nano - Text Editor",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["nano"] },
                        { Repository.Debian, ["nano"] },
                        { Repository.Fedora, ["nano"] },
                        { Repository.RedHat, ["nano"] },
                        { Repository.Ubuntu, ["nano"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "Node.js - JavaScript RE",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["nodejs", "npm"] },
                        { Repository.Debian, ["nodejs", "npm"] },
                        { Repository.Fedora, ["nodejs", "npm"] },
                        { Repository.RedHat, ["nodejs", "npm"] },
                        { Repository.Ubuntu, ["nodejs", "npm"] },
                    },
                    Flatpak = null,
                    Snap = new Snap("node", true, true, "18/stable"),
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "Podman - Containers",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["podman"] },
                        { Repository.Debian, ["podman"] },
                        { Repository.Fedora, ["podman"] },
                        { Repository.RedHat, ["podman"] },
                        { Repository.Ubuntu, ["podman"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "Rust Language",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["rustup"] },
                        { Repository.Debian, ["rustc", "rustfmt", "cargo"] },
                        { Repository.Fedora, ["rust", "rustfmt", "cargo"] },
                        { Repository.RedHat, ["rust", "rustfmt", "cargo"] },
                        { Repository.Ubuntu, ["rustc", "rustfmt", "cargo"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "Snap",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Debian, ["snapd"] },
                        { Repository.Fedora, ["snapd"] },
                        { Repository.RedHat, ["snapd"] },
                        { Repository.Ubuntu, ["snapd"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "SSH - Secure Shell Protocol",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["libssh", "openssh"] },
                        { Repository.Debian, ["ssh"] },
                        { Repository.Fedora, ["libssh", "openssh"] },
                        { Repository.RedHat, ["libssh", "openssh"] },
                        { Repository.Ubuntu, ["ssh"] },
                    },
                    Flatpak = null,
                    Snap = null,
                    PreInstall = null,
                    PostInstall = null,
                },
                new Package()
                {
                    Name = "vim - Text Editor",
                    DesktopEnvironment = null,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        {
                            Repository.Arch,
                            ["vim", "vim-airline", "vim-ale", "vim-ctrlp", "vim-gitgutter", "vim-nerdtree"]
                        },
                        {
                            Repository.Debian,
                            ["vim", "vim-airline", "vim-ale", "vim-ctrlp", "vim-gitgutter"]
                        },
                        {
                            Repository.Fedora,
                            ["vim-enhanced", "vim-airline", "vim-ale", "vim-ctrlp", "vim-gitgutter", "vim-nerdtree"]
                        },
                        {
                            Repository.RedHat,
                            ["vim-enhanced"]
                        },
                        {
                            Repository.Ubuntu,
                            ["vim", "vim-airline", "vim-ale", "vim-ctrlp", "vim-gitgutter"]
                        },
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