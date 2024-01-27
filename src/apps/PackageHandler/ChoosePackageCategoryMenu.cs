using Linux.Enums;
using Linux;
using PackageHandler.Enums;
using Spectre.Console;
using System.Collections.Generic;
using System;
using System.IO;
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
                new Package
                {
                    Name = "Cockpit - Web Interface",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["cockpit"] },
                        { Repository.Debian, ["cockpit"] },
                        { Repository.Fedora, ["cockpit"] },
                        { Repository.RedHat, ["cockpit"] },
                        { Repository.Ubuntu, ["cockpit"] },
                    },
                },
                new Package
                {
                    Name = "cURL - Client URL",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["curl"] },
                        { Repository.Debian, ["curl"] },
                        { Repository.Fedora, ["curl"] },
                        { Repository.RedHat, ["curl"] },
                        { Repository.Ubuntu, ["curl"] },
                    },
                },
                new Package
                {
                    Name = "dotnet - C# runtime 8.0 LTS",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["dotnet-runtime-8.0"] },
                        { Repository.Debian, ["dotnet-runtime-8.0"] },
                        { Repository.Fedora, ["dotnet-runtime-8.0"] },
                        { Repository.RedHat, ["dotnet-runtime-8.0"] },
                        { Repository.Ubuntu, ["dotnet-runtime-8.0"] },
                    },
                    Snap = new Snap("dotnet-runtime-80", true),
                    PreInstall = (distribution, method) =>
                    {
                        if (method == InstallMethod.Repository)
                        {
                            if (distribution.Repository == Repository.Debian)
                            {
                                distribution.Install("wget");
                                new Command(
                                        "wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb")
                                    .Run();
                                new Command("sudo dpkg -i packages-microsoft-prod.deb").Run();
                                new Command("rm packages-microsoft-prod.deb").Run();
                                distribution.Update();
                            }
                        }
                    },
                },
                new Package
                {
                    Name = "dotnet - C# SDK 8.0 LTS",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["dotnet-sdk-8.0"] },
                        { Repository.Debian, ["dotnet-sdk-8.0"] },
                        { Repository.Fedora, ["dotnet-sdk-8.0"] },
                        { Repository.RedHat, ["dotnet-sdk-8.0"] },
                        { Repository.Ubuntu, ["dotnet-sdk-8.0"] },
                    },
                    Snap = new Snap("dotnet-sdk", true, true, "8.0/stable"),
                    PreInstall = (distribution, method) =>
                    {
                        if (method == InstallMethod.Repository)
                        {
                            if (distribution.Repository == Repository.Debian)
                            {
                                distribution.Install("wget");
                                new Command(
                                        "wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb")
                                    .Run();
                                new Command("sudo dpkg -i packages-microsoft-prod.deb").Run();
                                new Command("rm packages-microsoft-prod.deb").Run();
                                distribution.Update();
                            }
                        }
                    },
                },
                new Package
                {
                    Name = "Flatpak",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["flatpak"] },
                        { Repository.Debian, ["flatpak"] },
                        { Repository.Fedora, ["flatpak"] },
                        { Repository.RedHat, ["flatpak"] },
                        { Repository.Ubuntu, ["flatpak"] },
                    },
                },
                new Package
                {
                    Name = "Flutter",
                    Snap = new Snap("flutter", true, true),
                },
                new Package
                {
                    Name = "git - Version Control",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["git"] },
                        { Repository.Debian, ["git"] },
                        { Repository.Fedora, ["git"] },
                        { Repository.RedHat, ["git"] },
                        { Repository.Ubuntu, ["git"] },
                    },
                },
                new Package
                {
                    Name = "Go Language",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["go", "gopls"] },
                        { Repository.Debian, ["golang", "gopls"] },
                        { Repository.Fedora, ["golang", "golang-x-tools-gopls"] },
                        { Repository.RedHat, ["golang"] },
                        { Repository.Ubuntu, ["golang", "gopls"] },
                    },
                    Snap = new Snap("go", true, true),
                    PreInstall = (_, method) =>
                    {
                        if (method == InstallMethod.Uninstall)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            new Command($"sudo rm -r {Path.Combine(homeDirectory, ".go")}")
                                .HideOutput(true)
                                .Run();
                        }
                    },
                    PostInstall = (distribution, method) =>
                    {
                        if (method != InstallMethod.Uninstall)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            new Command($"go env -w GOPATH={Path.Combine(homeDirectory, ".go")}").Run();
                            if (method == InstallMethod.Snap || distribution.Repository == Repository.RedHat)
                            {
                                var bashrc = Path.Combine(homeDirectory, ".bashrc");
                                if (!File.Exists(bashrc) ||
                                    !File.ReadAllText(bashrc).Contains("export GOPATH"))
                                {
                                    File.AppendText("export GOPATH=$HOME/.go");
                                    File.AppendText("export PATH=$PATH:$GOPATH/bin");
                                }

                                new Command("go install golang.org/x/tools/gopls@latest").Run();
                            }
                        }
                    },
                },
                new Package
                {
                    Name = "htop - Process Reviewer",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["htop"] },
                        { Repository.Debian, ["htop"] },
                        { Repository.Fedora, ["htop"] },
                        { Repository.RedHat, ["htop"] },
                        { Repository.Ubuntu, ["htop"] },
                    },
                },
                new Package
                {
                    Name = "MariaDB - Database",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["mariadb"] },
                        { Repository.Debian, ["mariadb-server"] },
                        { Repository.Fedora, ["mariadb-server"] },
                        { Repository.RedHat, ["mariadb-server"] },
                        { Repository.Ubuntu, ["mariadb-server"] },
                    },
                },
                new Package
                {
                    Name = "neovim - Text Editor",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["neovim"] },
                        { Repository.Debian, ["neovim"] },
                        { Repository.Fedora, ["neovim"] },
                        { Repository.Ubuntu, ["neovim"] },
                    },
                    PreInstall = (_, method) =>
                    {
                        if (method == InstallMethod.Uninstall)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            new Command($"sudo rm -r {Path.Combine(homeDirectory, ".config/nvim")}")
                                .HideOutput(true)
                                .Run();
                        }
                    },
                    PostInstall = (distribution, method) =>
                    {
                        if (method != InstallMethod.Uninstall)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            var configFile = Path.Combine(homeDirectory, ".config/nvim/init.vim");
                            File.WriteAllText(configFile, """
                                                          " neovim settings

                                                          set noswapfile
                                                          set nobackup
                                                          set nowritebackup

                                                          set updatetime=300
                                                          set scrolloff=10
                                                          set number
                                                          set relativenumber
                                                          set ignorecase smartcase
                                                          set incsearch hlsearch
                                                          set foldmethod=indent
                                                          set foldlevel=99

                                                          syntax on
                                                          colorscheme desert
                                                          filetype plugin indent on

                                                          " normal mode remaps

                                                          let mapleader = " "

                                                          " window split
                                                          nnoremap <Leader>vs <C-w>v
                                                          nnoremap <Leader>hs <C-w>s

                                                          " window navigation
                                                          nnoremap <C-h> <C-w>h
                                                          nnoremap <C-j> <C-w>j
                                                          nnoremap <C-k> <C-w>k
                                                          nnoremap <C-l> <C-w>l

                                                          " text insert
                                                          nnoremap <Leader>go iif err != nil {}<ESC>

                                                          " file explore
                                                          nnoremap <Leader>ex :Explore<CR>
                                                          """);
                            if (distribution.Repository != Repository.RedHat)
                            {
                                if (distribution.Repository is Repository.Arch or Repository.Fedora)
                                {
                                    File.AppendAllText(configFile, "nnoremap <C-n> :NERDTreeToggle<CR>");
                                }

                                File.AppendAllText(configFile, """
                                                               " ale settings

                                                               let g:ale_fix_on_save = 1
                                                               let g:ale_completion_enabled = 1
                                                               let g:ale_linters = { "go": ["gopls"], "rust": ["analyzer"] }
                                                               let g:ale_fixers = { "*": ["remove_trailing_lines", "trim_whitespace"], "go": ["gofmt"], "rust": ["rustfmt"] }

                                                               nnoremap K :ALEHover<CR>
                                                               nnoremap gd :ALEGoToDefinition<CR>
                                                               nnoremap gn :ALERename<CR>
                                                               nnoremap gr :ALEFindReferences<CR>

                                                               " insert mode remaps

                                                               inoremap <silent><expr> <Tab> pumvisible() ? "\<C-n>" : "\<TAB>"
                                                               inoremap <silent><expr> <S-Tab> pumvisible() ? "\<C-n>" : "\<S-TAB>"
                                                               """);
                            }
                        }
                    },
                },
                new Package
                {
                    Name = "nano - Text Editor",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["nano"] },
                        { Repository.Debian, ["nano"] },
                        { Repository.Fedora, ["nano"] },
                        { Repository.RedHat, ["nano"] },
                        { Repository.Ubuntu, ["nano"] },
                    },
                },
                new Package
                {
                    Name = "Node.js - JavaScript RE",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["nodejs", "npm"] },
                        { Repository.Debian, ["nodejs", "npm"] },
                        { Repository.Fedora, ["nodejs", "npm"] },
                        { Repository.RedHat, ["nodejs", "npm"] },
                        { Repository.Ubuntu, ["nodejs", "npm"] },
                    },
                    Snap = new Snap("node", true, true, "18/stable"),
                    PreInstall = (distribution, method) =>
                    {
                        if (method == InstallMethod.Repository)
                        {
                            if (distribution.Repository == Repository.RedHat)
                            {
                                new Command("sudo dnf module enable nodejs:20 -y").Run();
                            }
                        }
                    },
                },
                new Package
                {
                    Name = "Podman - Containers",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["podman"] },
                        { Repository.Debian, ["podman"] },
                        { Repository.Fedora, ["podman"] },
                        { Repository.RedHat, ["podman"] },
                        { Repository.Ubuntu, ["podman"] },
                    },
                },
                new Package
                {
                    Name = "Rust Language",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["rustup"] },
                        { Repository.Debian, ["rustc", "rustfmt", "cargo"] },
                        { Repository.Fedora, ["rust", "rustfmt", "cargo"] },
                        { Repository.RedHat, ["rust", "rustfmt", "cargo"] },
                        { Repository.Ubuntu, ["rustc", "rustfmt", "cargo"] },
                    },
                    Other = new Other(OtherPackage.Rust),
                    PreInstall = (distribution, method) =>
                    {
                        if (method != InstallMethod.Other)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            new Command($"sudo rm -r {Path.Combine(homeDirectory, ".cargo/bin/rustup")}")
                                .HideOutput(true)
                                .Run();
                        }

                        if (method == InstallMethod.Other)
                        {
                            distribution.Install("curl");
                        }
                    },
                    PostInstall = (_, method) =>
                    {
                        if (method == InstallMethod.Other)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            new Command(
                                    $"{Path.Combine(homeDirectory, ".cargo/bin/rustup")} component add rust-analyzer")
                                .Run();
                        }
                    },
                },
                new Package
                {
                    Name = "Snap",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Debian, ["snapd"] },
                        { Repository.Fedora, ["snapd"] },
                        { Repository.RedHat, ["snapd"] },
                        { Repository.Ubuntu, ["snapd"] },
                    },
                    PostInstall = (distribution, method) =>
                    {
                        if (method != InstallMethod.Uninstall)
                        {
                            Snap.Setup(distribution);
                        }
                    },
                },
                new Package
                {
                    Name = "SSH - Secure Shell Protocol",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["libssh", "openssh"] },
                        { Repository.Debian, ["ssh"] },
                        { Repository.Fedora, ["libssh", "openssh"] },
                        { Repository.RedHat, ["libssh", "openssh"] },
                        { Repository.Ubuntu, ["ssh"] },
                    },
                },
                new Package
                {
                    Name = "vim - Text Editor",
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
                    PreInstall = (_, method) =>
                    {
                        if (method == InstallMethod.Uninstall)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            new Command(
                                    $"sudo rm -r {Path.Combine(homeDirectory, ".vim")} {Path.Combine(homeDirectory, ".viminfo")} {Path.Combine(homeDirectory, ".vimrc")}")
                                .HideOutput(true)
                                .Run();
                        }
                    },
                    PostInstall = (distribution, method) =>
                    {
                        if (method != InstallMethod.Uninstall)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            var bashrc = Path.Combine(homeDirectory, ".bashrc");
                            if (!File.Exists(bashrc) || !File.ReadAllText(bashrc).Contains("export EDITOR"))
                            {
                                File.AppendAllText(bashrc, "export EDITOR=\"/usr/bin/vim\"\n");
                            }

                            var configFile = Path.Combine(homeDirectory, ".vimrc");
                            File.WriteAllText(configFile, """
                                                          " vim settings

                                                          set nocompatible

                                                          set encoding=utf-8

                                                          set noswapfile
                                                          set nobackup
                                                          set nowritebackup

                                                          set mouse=a
                                                          set updatetime=300
                                                          set scrolloff=10
                                                          set number
                                                          set relativenumber
                                                          set ignorecase smartcase
                                                          set incsearch hlsearch
                                                          set foldmethod=indent
                                                          set foldlevel=99

                                                          syntax on
                                                          colorscheme desert
                                                          filetype plugin indent on

                                                          " normal mode remaps

                                                          let mapleader = " "

                                                          " window split
                                                          nnoremap <Leader>vs <C-w>v
                                                          nnoremap <Leader>hs <C-w>s

                                                          " window navigation
                                                          nnoremap <C-h> <C-w>h
                                                          nnoremap <C-j> <C-w>j
                                                          nnoremap <C-k> <C-w>k
                                                          nnoremap <C-l> <C-w>l

                                                          " text insert
                                                          nnoremap <Leader>go iif err != nil {}<ESC>

                                                          " file explore
                                                          nnoremap <Leader>ex :Explore<CR>
                                                          """);
                            if (distribution.Repository != Repository.RedHat)
                            {
                                if (distribution.Repository is Repository.Arch or Repository.Fedora)
                                {
                                    File.AppendAllText(configFile, "nnoremap <C-n> :NERDTreeToggle<CR>");
                                }

                                File.AppendAllText(configFile, """
                                                               " ale settings

                                                               let g:ale_fix_on_save = 1
                                                               let g:ale_completion_enabled = 1
                                                               let g:ale_linters = { "go": ["gopls"], "rust": ["analyzer"] }
                                                               let g:ale_fixers = { "*": ["remove_trailing_lines", "trim_whitespace"], "go": ["gofmt"], "rust": ["rustfmt"] }

                                                               nnoremap K :ALEHover<CR>
                                                               nnoremap gd :ALEGoToDefinition<CR>
                                                               nnoremap gn :ALERename<CR>
                                                               nnoremap gr :ALEFindReferences<CR>

                                                               " insert mode remaps

                                                               inoremap <silent><expr> <Tab> pumvisible() ? "\<C-n>" : "\<TAB>"
                                                               inoremap <silent><expr> <S-Tab> pumvisible() ? "\<C-n>" : "\<S-TAB>"
                                                               """);
                            }
                        }
                    },
                },
            ],
            PackageCategory.Desktop =>
            [
                new Package
                {
                    Name = "cups - Printer Support",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["cups"] },
                        { Repository.Debian, ["cups"] },
                        { Repository.Fedora, ["cups"] },
                        { Repository.RedHat, ["cups"] },
                        { Repository.Ubuntu, ["cups"] },
                    },
                },
                new Package
                {
                    Name = "ffmpeg - Media Codecs",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["ffmpeg"] },
                        { Repository.Debian, ["ffmpeg"] },
                        { Repository.Fedora, ["ffmpeg"] },
                        { Repository.RedHat, ["ffmpeg"] },
                        { Repository.Ubuntu, ["ffmpeg"] },
                    },
                },
                new Package
                {
                    Name = "imagemagick",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["imagemagick"] },
                        { Repository.Debian, ["imagemagick"] },
                        { Repository.Fedora, ["ImageMagick"] },
                        { Repository.RedHat, ["ImageMagick"] },
                        { Repository.Ubuntu, ["imagemagick"] },
                    },
                },
                new Package
                {
                    Name = "LaTex - Compiler",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["texlive-core", "texlive-latexextra"] },
                        { Repository.Debian, ["texlive-latex-base", "texlive-latex-extra"] },
                        { Repository.Fedora, ["texlive-latex", "texlive-collection-latexextra"] },
                        { Repository.RedHat, ["texlive-latex"] },
                        { Repository.Ubuntu, ["texlive-latex-base", "texlive-latex-extra"] },
                    },
                },
                new Package
                {
                    Name = "MP3 Metadata Editor",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["id3v2"] },
                        { Repository.Debian, ["id3v2"] },
                        { Repository.Fedora, ["id3v2"] },
                        { Repository.Ubuntu, ["id3v2"] },
                    },
                },
                new Package
                {
                    Name = "Vietnamese Keyboard",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["ibus-unikey"] },
                        { Repository.Debian, ["ibus-unikey"] },
                        { Repository.Fedora, ["ibus-unikey"] },
                        {
                            Repository.RedHat,
                            [
                                "https://rpmfind.net/linux/fedora/linux/releases/34/Everything/x86_64/os/Packages/i/ibus-unikey-0.6.1-26.20190311git46b5b9e.fc34.x86_64.rpm"
                            ]
                        },
                        { Repository.Ubuntu, ["ibus-unikey"] },
                    },
                },
                new Package
                {
                    Name = "yt-dlp - Download YouTube",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["yt-dlp"] },
                        { Repository.Debian, ["yt-dlp"] },
                        { Repository.Fedora, ["yt-dlp"] },
                        { Repository.RedHat, ["yt-dlp"] },
                        { Repository.Ubuntu, ["yt-dlp"] },
                    },
                },
            ],
            PackageCategory.Applications =>
            [
                new Package
                {
                    Name = "Cheese - Webcam",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["cheese"] },
                        { Repository.Debian, ["cheese"] },
                        { Repository.Fedora, ["cheese"] },
                        { Repository.RedHat, ["cheese"] },
                        { Repository.Ubuntu, ["cheese"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Cheese", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Deja Dup - Backups",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["deja-dup"] },
                        { Repository.Debian, ["deja-dup"] },
                        { Repository.Fedora, ["deja-dup"] },
                        { Repository.Ubuntu, ["deja-dup"] },
                    },
                    Flatpak = new Flatpak("org.gnome.DejaDup", [FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Evince - Document Viewer",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["evince"] },
                        { Repository.Debian, ["evince"] },
                        { Repository.Fedora, ["evince"] },
                        { Repository.RedHat, ["evince"] },
                        { Repository.Ubuntu, ["evince"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Evince", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Eye of Gnome - Image Viewer",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["eog"] },
                        { Repository.Debian, ["eog"] },
                        { Repository.Fedora, ["eog"] },
                        { Repository.RedHat, ["eog"] },
                        { Repository.Ubuntu, ["eog"] },
                    },
                    Flatpak = new Flatpak("org.gnome.eog", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("eog", true),
                },
                new Package
                {
                    Name = "Gnome Boxes - VM Manager",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-boxes"] },
                        { Repository.Debian, ["gnome-boxes"] },
                        { Repository.Fedora, ["gnome-boxes"] },
                        { Repository.Ubuntu, ["gnome-boxes"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Boxes", [FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Calculator",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-calculator"] },
                        { Repository.Debian, ["gnome-calculator"] },
                        { Repository.Fedora, ["gnome-calculator"] },
                        { Repository.RedHat, ["gnome-calculator"] },
                        { Repository.Ubuntu, ["gnome-calculator"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Calculator", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("gnome-calculator", true)
                },
                new Package
                {
                    Name = "Gnome Calendar",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-calendar"] },
                        { Repository.Debian, ["gnome-calendar"] },
                        { Repository.Fedora, ["gnome-calendar"] },
                        { Repository.Ubuntu, ["gnome-calendar"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Calendar", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Clocks",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-clocks"] },
                        { Repository.Debian, ["gnome-clocks"] },
                        { Repository.Fedora, ["gnome-clocks"] },
                        { Repository.Ubuntu, ["gnome-clocks"] },
                    },
                    Flatpak = new Flatpak("org.gnome.clocks", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("gnome-clocks", true)
                },
                new Package
                {
                    Name = "Gnome Connections",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-connections"] },
                        { Repository.Debian, ["gnome-connections"] },
                        { Repository.Fedora, ["gnome-connections"] },
                        { Repository.RedHat, ["gnome-connections"] },
                        { Repository.Ubuntu, ["gnome-connections"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Connections", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Contacts",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-contacts"] },
                        { Repository.Debian, ["gnome-contacts"] },
                        { Repository.Fedora, ["gnome-contacts"] },
                        { Repository.Ubuntu, ["gnome-contacts"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Contacts", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Image Viewer",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Fedora, ["loupe"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Loupe", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("loupe", true)
                },
                new Package
                {
                    Name = "Gnome Maps",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-maps"] },
                        { Repository.Debian, ["gnome-maps"] },
                        { Repository.Fedora, ["gnome-maps"] },
                        { Repository.Ubuntu, ["gnome-maps"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Maps", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Password Safe",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-passwordsafe"] },
                        { Repository.Debian, ["gnome-passwordsafe"] },
                        { Repository.Fedora, ["secrets"] },
                        { Repository.Ubuntu, ["gnome-passwordsafe"] },
                    },
                    Flatpak = new Flatpak("org.gnome.World.Secrets", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Weather",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-weather"] },
                        { Repository.Debian, ["gnome-weather"] },
                        { Repository.Fedora, ["gnome-weather"] },
                        { Repository.Ubuntu, ["gnome-weather"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Weather", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "GNU Cash - Accounting",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnucash"] },
                        { Repository.Debian, ["gnucash"] },
                        { Repository.Fedora, ["gnucash"] },
                        { Repository.Ubuntu, ["gnucash"] },
                    },
                    Flatpak = new Flatpak("org.gnucash.GnuCash", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gwenview - Image Viewer",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gwenview"] },
                        { Repository.Debian, ["gwenview"] },
                        { Repository.Fedora, ["gwenview"] },
                        { Repository.Ubuntu, ["gwenview"] },
                    },
                    Flatpak = new Flatpak("org.kde.gwenview", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("gwenview", true)
                },
                new Package
                {
                    Name = "KCalc - Calculator",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["kcalc"] },
                        { Repository.Debian, ["kcalc"] },
                        { Repository.Fedora, ["kcalc"] },
                        { Repository.RedHat, ["kcalc"] },
                        { Repository.Ubuntu, ["kcalc"] },
                    },
                    Flatpak = new Flatpak("org.kde.kcalc", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("kcalc", true)
                },
                new Package
                {
                    Name = "Okular - Document Viewer",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["okular"] },
                        { Repository.Debian, ["okular"] },
                        { Repository.Fedora, ["okular"] },
                        { Repository.Ubuntu, ["okular"] },
                    },
                    Flatpak = new Flatpak("org.kde.okular", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("okular", true)
                },
                new Package
                {
                    Name = "Transmission (GTK) - Torrent",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["transmission-gtk"] },
                        { Repository.Debian, ["transmission-gtk"] },
                        { Repository.Fedora, ["transmission-gtk"] },
                        { Repository.Ubuntu, ["transmission-gtk"] },
                    },
                    Flatpak = new Flatpak("com.transmissionbt.Transmission",
                        [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Transmission (QT) - Torrent",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["transmission-qt"] },
                        { Repository.Debian, ["transmission-qt"] },
                        { Repository.Fedora, ["transmission-qt"] },
                        { Repository.Ubuntu, ["transmission-qt"] },
                    },
                    Flatpak = new Flatpak("com.transmissionbt.Transmission",
                        [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Virt Manager",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["virt-manager"] },
                        { Repository.Debian, ["virt-manager"] },
                        { Repository.Fedora, ["virt-manager"] },
                        { Repository.RedHat, ["virt-manager"] },
                        { Repository.Ubuntu, ["virt-manager"] },
                    },
                },
            ],
            PackageCategory.Browsers =>
            [
                new Package
                {
                    Name = "Chromium",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["chromium"] },
                        { Repository.Debian, ["chromium"] },
                        { Repository.Fedora, ["chromium"] },
                    },
                    Flatpak = new Flatpak("org.chromium.Chromium", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("chromium", true)
                },
                new Package
                {
                    Name = "Epiphany - Gnome Web",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["epiphany"] },
                        { Repository.Debian, ["epiphany-browser"] },
                        { Repository.Fedora, ["epiphany"] },
                        { Repository.Ubuntu, ["epiphany-browser"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Epiphany", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Firefox",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["firefox"] },
                        { Repository.Fedora, ["firefox"] },
                    },
                    Flatpak = new Flatpak("org.mozilla.firefox", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("firefox", true)
                },
                new Package
                {
                    Name = "Firefox ESR",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Debian, ["firefox-esr"] },
                        { Repository.RedHat, ["firefox"] },
                    },
                    Snap = new Snap("firefox", true, false, "esr-stable"),
                },
                new Package
                {
                    Name = "IceCat - GNU Browser",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Fedora, ["icecat"] },
                    },
                },
                new Package
                {
                    Name = "TOR - The Onion Router",
                    Flatpak = new Flatpak("com.github.micahflee.torbrowser-launcher", [FlatpakRemote.FlatHub]),
                },
            ],
            PackageCategory.Communication =>
            [
                new Package
                {
                    Name = "Discord",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["discord"] }
                    },
                    Flatpak = new Flatpak("com.discordapp.Discord", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("discord")
                },
                new Package
                {
                    Name = "Thunderbird",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["thunderbird"] },
                        { Repository.Debian, ["thunderbird"] },
                        { Repository.Fedora, ["thunderbird"] },
                        { Repository.RedHat, ["thunderbird"] },
                        { Repository.Ubuntu, ["thunderbird"] },
                    },
                    Flatpak = new Flatpak("org.mozilla.Thunderbird", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("thunderbird", true)
                },
            ],
            PackageCategory.Games =>
            [
                new Package
                {
                    Name = "0 A.D.",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["0ad"] },
                        { Repository.Debian, ["0ad"] },
                        { Repository.Fedora, ["0ad"] },
                        { Repository.Ubuntu, ["0ad"] },
                    },
                    Flatpak = new Flatpak("com.play0ad.zeroad", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("0ad", true)
                },
                new Package
                {
                    Name = "Gnome 2048",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-2048"] },
                        { Repository.Debian, ["gnome-2048"] },
                        { Repository.Fedora, ["gnome-2048"] },
                        { Repository.Ubuntu, ["gnome-2048"] },
                    },
                    Flatpak = new Flatpak("org.gnome.TwentyFortyEight", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Chess",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-chess"] },
                        { Repository.Debian, ["gnome-chess"] },
                        { Repository.Fedora, ["gnome-chess"] },
                        { Repository.Ubuntu, ["gnome-chess"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Chess", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Mines",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-mines"] },
                        { Repository.Debian, ["gnome-mines"] },
                        { Repository.Fedora, ["gnome-mines"] },
                        { Repository.Ubuntu, ["gnome-mines"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Mines", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Solitaire",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["aisleriot"] },
                        { Repository.Debian, ["aisleriot"] },
                        { Repository.Fedora, ["aisleriot"] },
                        { Repository.Ubuntu, ["aisleriot"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Aisleriot", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Sudoku",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-sudoku"] },
                        { Repository.Debian, ["gnome-sudoku"] },
                        { Repository.Fedora, ["gnome-sudoku"] },
                        { Repository.Ubuntu, ["gnome-sudoku"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Sudoku", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("gnome-sudoku", true)
                },
                new Package
                {
                    Name = "Gnome Tetris",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["quadrapassel"] },
                        { Repository.Debian, ["quadrapassel"] },
                        { Repository.Fedora, ["quadrapassel"] },
                        { Repository.Ubuntu, ["quadrapassel"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Quadrapassel", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("quadrapassel", true)
                },
                new Package
                {
                    Name = "KDE Chess",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["knights"] },
                        { Repository.Debian, ["knights"] },
                        { Repository.Fedora, ["knights"] },
                        { Repository.Ubuntu, ["knights"] },
                    },
                    Snap = new Snap("knights", true)
                },
                new Package
                {
                    Name = "KDE Mines",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["kmines"] },
                        { Repository.Debian, ["kmines"] },
                        { Repository.Fedora, ["kmines"] },
                        { Repository.RedHat, ["kmines"] },
                        { Repository.Ubuntu, ["kmines"] },
                    },
                    Snap = new Snap("kmines", true)
                },
                new Package
                {
                    Name = "KDE Sudoku",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["ksudoku"] },
                        { Repository.Debian, ["ksudoku"] },
                        { Repository.Fedora, ["ksudoku"] },
                        { Repository.RedHat, ["ksudoku"] },
                        { Repository.Ubuntu, ["ksudoku"] },
                    },
                    Flatpak = new Flatpak("org.kde.ksudoku", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("ksudoku", true)
                },
                new Package
                {
                    Name = "Steam",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["steam"] },
                    },
                    Flatpak = new Flatpak("com.valvesoftware.Steam", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("steam", true)
                },
                new Package
                {
                    Name = "Super Tux Kart",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["supertuxkart"] },
                        { Repository.Debian, ["supertuxkart"] },
                        { Repository.Fedora, ["supertuxkart"] },
                        { Repository.Ubuntu, ["supertuxkart"] },
                    },
                    Flatpak = new Flatpak("net.supertuxkart.SuperTuxKart",
                        [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("supertuxkart")
                },
                new Package
                {
                    Name = "Xonotic",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["xonotic"] },
                        { Repository.Fedora, ["xonotic"] },
                    },
                    Flatpak = new Flatpak("org.xonotic.Xonotic", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("xonotic")
                },
            ],
            PackageCategory.MultiMedia =>
            [
                new Package
                {
                    Name = "Blender",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["blender"] },
                        { Repository.Debian, ["blender"] },
                        { Repository.Fedora, ["blender"] },
                        { Repository.Ubuntu, ["blender"] },
                    },
                    Flatpak = new Flatpak("org.blender.Blender", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("blender", true, true)
                },
                new Package
                {
                    Name = "Elisa Music Player",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["elisa"] },
                        { Repository.Debian, ["elisa"] },
                        { Repository.Fedora, ["elisa"] },
                        { Repository.Ubuntu, ["elisa"] },
                    },
                    Flatpak = new Flatpak("org.kde.elisa", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "GIMP",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gimp"] },
                        { Repository.Debian, ["gimp"] },
                        { Repository.Fedora, ["gimp"] },
                        { Repository.RedHat, ["gimp"] },
                        { Repository.Ubuntu, ["gimp"] },
                    },
                    Flatpak = new Flatpak("org.gimp.GIMP", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("gimp")
                },
                new Package
                {
                    Name = "Gnome Music",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-music"] },
                        { Repository.Debian, ["gnome-music"] },
                        { Repository.Fedora, ["gnome-music"] },
                        { Repository.Ubuntu, ["gnome-music"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Music", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Photos",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-photos"] },
                        { Repository.Debian, ["gnome-photos"] },
                        { Repository.Fedora, ["gnome-photos"] },
                        { Repository.RedHat, ["gnome-photos"] },
                        { Repository.Ubuntu, ["gnome-photos"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Photos", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Sound Recorder",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-sound-recorder"] },
                        { Repository.Debian, ["gnome-sound-recorder"] },
                        { Repository.Fedora, ["gnome-sound-recorder"] },
                        { Repository.Ubuntu, ["gnome-sound-recorder"] },
                    },
                    Flatpak = new Flatpak("org.gnome.SoundRecorder", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "KdenLive Video Editor",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["kdenlive"] },
                        { Repository.Debian, ["kdenlive"] },
                        { Repository.Fedora, ["kdenlive"] },
                        { Repository.Ubuntu, ["kdenlive"] },
                    },
                    Flatpak = new Flatpak("org.kde.kdenlive", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("kdenlive", true)
                },
                new Package
                {
                    Name = "RhythmBox",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["rhythmbox"] },
                        { Repository.Debian, ["rhythmbox"] },
                        { Repository.Fedora, ["rhythmbox"] },
                        { Repository.Ubuntu, ["rhythmbox"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Rhythmbox3", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Shotwell",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["shotwell"] },
                        { Repository.Debian, ["shotwell"] },
                        { Repository.Fedora, ["shotwell"] },
                        { Repository.Ubuntu, ["shotwell"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Shotwell", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Totem Video Player",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["totem"] },
                        { Repository.Debian, ["totem"] },
                        { Repository.Fedora, ["totem"] },
                        { Repository.RedHat, ["totem"] },
                        { Repository.Ubuntu, ["totem"] },
                    },
                    Flatpak = new Flatpak("org.gnome.Totem", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "VLC",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["vlc"] },
                        { Repository.Debian, ["vlc"] },
                        { Repository.Fedora, ["vlc"] },
                        { Repository.RedHat, ["vlc"] },
                        { Repository.Ubuntu, ["vlc"] },
                    },
                    Flatpak = new Flatpak("org.videolan.VLC", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("vlc", true)
                },
            ],
            PackageCategory.Editors =>
            [
                new Package
                {
                    Name = "gedit",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gedit"] },
                        { Repository.Debian, ["gedit"] },
                        { Repository.Fedora, ["gedit"] },
                        { Repository.RedHat, ["gedit"] },
                        { Repository.Ubuntu, ["gedit"] },
                    },
                    Flatpak = new Flatpak("org.gnome.gedit", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("gedit", true)
                },
                new Package
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
                },
                new Package
                {
                    Name = "Gnome Text Editor",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-text-editor"] },
                        { Repository.Debian, ["gnome-text-editor"] },
                        { Repository.Fedora, ["gnome-text-editor"] },
                        { Repository.Ubuntu, ["gnome-text-editor"] },
                    },
                    Flatpak = new Flatpak("org.gnome.TextEditor", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Intellij",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["intellij-idea-community-edition"] },
                    },
                    Flatpak = new Flatpak("com.jetbrains.IntelliJ-IDEA-Community", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("intellij-idea-community", true, true),
                    PostInstall = (_, method) =>
                    {
                        if (method != InstallMethod.Uninstall)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            var configFile = Path.Combine(homeDirectory, ".ideavimrc");
                            File.WriteAllText(configFile, "sethandler a:ide");
                        }
                    },
                },
                new Package
                {
                    Name = "Kate",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["kate"] },
                        { Repository.Debian, ["kate"] },
                        { Repository.Fedora, ["kate"] },
                        { Repository.RedHat, ["kate"] },
                        { Repository.Ubuntu, ["kate"] },
                    },
                    Snap = new Snap("kate", true, true)
                },
                new Package
                {
                    Name = "KDevelop",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["kdevelop"] },
                        { Repository.Debian, ["kdevelop"] },
                        { Repository.Fedora, ["kdevelop"] },
                        { Repository.Ubuntu, ["kdevelop"] },
                    },
                    Flatpak = new Flatpak("org.kde.kdevelop", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("kdevelop", true, true)
                },
                new Package
                {
                    Name = "Kile - LaTex Editor",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["kile"] },
                        { Repository.Debian, ["kile"] },
                        { Repository.Fedora, ["kile"] },
                        { Repository.Ubuntu, ["kile"] },
                    },
                },
                new Package
                {
                    Name = "KWrite",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["kwrite"] },
                        { Repository.Debian, ["kwrite"] },
                        { Repository.Fedora, ["kwrite"] },
                        { Repository.RedHat, ["kwrite"] },
                        { Repository.Ubuntu, ["kwrite"] },
                    },
                    Flatpak = new Flatpak("org.kde.kwrite", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "LibreOffice",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        {
                            Repository.Arch,
                            ["libreoffice-fresh"]
                        },
                        {
                            Repository.Debian,
                            ["libreoffice-writer", "libreoffice-calc", "libreoffice-impress", "libreoffice-draw"]
                        },
                        {
                            Repository.Fedora,
                            ["libreoffice-writer", "libreoffice-calc", "libreoffice-impress", "libreoffice-draw"]
                        },
                        {
                            Repository.RedHat,
                            ["libreoffice-writer", "libreoffice-calc", "libreoffice-impress", "libreoffice-draw"]
                        },
                        {
                            Repository.Ubuntu,
                            ["libreoffice-writer", "libreoffice-calc", "libreoffice-impress", "libreoffice-draw"]
                        },
                    },
                    Flatpak = new Flatpak("org.libreoffice.LibreOffice", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("libreoffice", true)
                },
                new Package
                {
                    Name = "Pycharm",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["pycharm-community-edition"] },
                        { Repository.Fedora, ["pycharm-community"] },
                    },
                    Flatpak = new Flatpak("com.jetbrains.PyCharm-Community", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("pycharm-community", true, true),
                    PreInstall = (distribution, method) =>
                    {
                        if (method != InstallMethod.Repository)
                        {
                            if (distribution.Repository == Repository.Fedora)
                            {
                                new Command("sudo dnf config-manager --set-disabled phracek-PyCharm").Run();
                            }
                        }

                        if (method == InstallMethod.Repository)
                        {
                            if (distribution.Repository == Repository.Fedora)
                            {
                                new Command("sudo dnf config-manager --set-enabled phracek-PyCharm").Run();
                            }
                        }
                    },
                    PostInstall = (_, method) =>
                    {
                        if (method != InstallMethod.Uninstall)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            var configFile = Path.Combine(homeDirectory, ".ideavimrc");
                            File.WriteAllText(configFile, "sethandler a:ide");
                        }
                    },
                },
                new Package
                {
                    Name = "VS Code",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["code"] },
                        { Repository.Debian, ["code"] },
                        { Repository.Fedora, ["code"] },
                        { Repository.RedHat, ["code"] },
                    },
                    Flatpak = new Flatpak("com.visualstudio.code", [FlatpakRemote.FlatHub]),
                    Snap = new Snap("code", true, true),
                    PreInstall = (distribution, method) =>
                    {
                        if (method != InstallMethod.Repository)
                        {
                            if (distribution.PackageManager == PackageManager.Apt)
                            {
                                new Command("sudo rm /etc/apt/sources.list.d/vscode.list").Run();
                            }

                            if (distribution.PackageManager == PackageManager.Apt)
                            {
                                new Command("sudo dnf config-manager --set-disabled code").Run();
                                new Command("sudo rm /etc/yum.repos.d/vscode.repo").Run();
                            }
                        }

                        if (method == InstallMethod.Uninstall)
                        {
                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            new Command(
                                    $"sudo rm -r {Path.Combine(homeDirectory, ".vscode")} {Path.Combine(homeDirectory, ".config/Code")}")
                                .HideOutput(true)
                                .Run();
                        }


                        if (method == InstallMethod.Repository)
                        {
                            if (distribution.PackageManager == PackageManager.Apt)
                            {
                                const string fileName = "packages.microsoft";
                                distribution.Install("wget");
                                distribution.Install("gpg");
                                File.WriteAllText(
                                    fileName,
                                    new Command("wget -qO- https://packages.microsoft.com/keys/microsoft.asc")
                                        .GetOutput()
                                );
                                new Command("gpg --dearmor packages.microsoft").Run();
                                new Command(
                                        "sudo install -D -o root -g root -m 644 packages.microsoft.gpg /etc/apt/keyrings/packages.microsoft.gpg")
                                    .Run();
                                File.Delete(fileName);
                                File.Delete($"{fileName}.gpg");

                                new Command(
                                        "echo deb [arch=amd64,arm64,armhf signed-by=/etc/apt/keyrings/packages.microsoft.gpg] https://packages.microsoft.com/repos/code stable main")
                                    .PipeInto("sudo tee /etc/apt/sources.list.d/vscode.list");

                                distribution.Update();
                            }

                            if (distribution.PackageManager == PackageManager.Dnf)
                            {
                                new Command("sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc")
                                    .Run();
                                new Command(
                                        "echo -e [code]\\nname=Visual Studio Code\\nbaseurl=https://packages.microsoft.com/yumrepos/vscode\\nenabled=1\\ngpgcheck=1\\ngpgkey=https://packages.microsoft.com/keys/microsoft.asc")
                                    .PipeInto("sudo tee /etc/yum.repos.d/vscode.repo");
                            }
                        }
                    },
                    PostInstall = (_, method) =>
                    {
                        if (method != InstallMethod.Uninstall)
                        {
                            string[] extensions = ["esbenp.prettier-vscode", "vscodevim.vim"];
                            foreach (var extension in extensions)
                            {
                                new Command($"code --install-extension {extension}").Run();
                            }

                            var homeDirectory = Environment.GetEnvironmentVariable("HOME") ??
                                                throw new Exception("HOME could not be determined");
                            var configFile = Path.Combine(homeDirectory, ".config/Code/User/settings.json");
                            File.WriteAllText(configFile, """
                                                          {
                                                            "[css]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
                                                            "[html]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
                                                            "[javascript]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
                                                            "[json]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
                                                            "[jsonc]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
                                                            "[scss]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
                                                            "[typescript]": { "editor.defaultFormatter": "esbenp.prettier-vscode" },
                                                            "editor.formatOnSave": true,
                                                            "editor.rulers": [80, 160],
                                                            "extensions.ignoreRecommendations": true,
                                                            "git.openRepositoryInParentFolders": "always",
                                                            "telemetry.telemetryLevel": "off",
                                                            "vim.smartRelativeLine": true,
                                                            "vim.useCtrlKeys": false,
                                                            "workbench.startupEditor": "none"
                                                          }
                                                          """);
                        }
                    },
                },
            ],
            PackageCategory.Software =>
            [
                new Package
                {
                    Name = "Gnome Software",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-software"] },
                        { Repository.Debian, ["gnome-software"] },
                        { Repository.Fedora, ["gnome-software"] },
                        { Repository.RedHat, ["gnome-software"] },
                        { Repository.Ubuntu, ["gnome-software"] },
                    },
                },
                new Package
                {
                    Name = "Plasma Discover",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["discover"] },
                        { Repository.Debian, ["plasma-discover"] },
                        { Repository.Fedora, ["plasma-discover"] },
                        { Repository.Ubuntu, ["plasma-discover"] },
                    },
                },
                new Package
                {
                    Name = "Snap Store",
                    Snap = new Snap("snap-store", true)
                },
            ],
            PackageCategory.Utilities =>
            [
                new Package
                {
                    Name = "Ark Archiving",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["ark"] },
                        { Repository.Debian, ["ark"] },
                        { Repository.Fedora, ["ark"] },
                        { Repository.RedHat, ["ark"] },
                        { Repository.Ubuntu, ["ark"] },
                    },
                    Flatpak = new Flatpak("org.kde.ark", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                    Snap = new Snap("ark", true)
                },
                new Package
                {
                    Name = "dconf Editor",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["dconf-editor"] },
                        { Repository.Debian, ["dconf-editor"] },
                        { Repository.Fedora, ["dconf-editor"] },
                        { Repository.RedHat, ["dconf-editor"] },
                        { Repository.Ubuntu, ["dconf-editor"] },
                    },
                    Flatpak = new Flatpak("ca.desrt.dconf-editor", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Fedora Media Writer",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Fedora, ["mediawriter"] },
                    },
                    Flatpak = new Flatpak("org.fedoraproject.MediaWriter",
                        [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "FileLight Disk Usage",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["filelight"] },
                        { Repository.Debian, ["filelight"] },
                        { Repository.Fedora, ["filelight"] },
                        { Repository.RedHat, ["filelight"] },
                        { Repository.Ubuntu, ["filelight"] },
                    },
                },
                new Package
                {
                    Name = "GParted",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gparted"] },
                        { Repository.Debian, ["gparted"] },
                        { Repository.Fedora, ["gparted"] },
                        { Repository.RedHat, ["gparted"] },
                        { Repository.Ubuntu, ["gparted"] },
                    },
                },
                new Package
                {
                    Name = "Gnome Disk Usage",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["baobab"] },
                        { Repository.Debian, ["baobab"] },
                        { Repository.Fedora, ["baobab"] },
                        { Repository.RedHat, ["baobab"] },
                        { Repository.Ubuntu, ["baobab"] },
                    },
                    Flatpak = new Flatpak("org.gnome.baobab", [FlatpakRemote.Fedora, FlatpakRemote.FlatHub]),
                },
                new Package
                {
                    Name = "Gnome Disk Utility",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-disk-utility"] },
                        { Repository.Debian, ["gnome-disk-utility"] },
                        { Repository.Fedora, ["gnome-disk-utility"] },
                        { Repository.RedHat, ["gnome-disk-utility"] },
                        { Repository.Ubuntu, ["gnome-disk-utility"] },
                    },
                },
                new Package
                {
                    Name = "Gnome Shell Extension",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-shell-extensions"] },
                        { Repository.Debian, ["gnome-shell-extensions"] },
                        { Repository.Fedora, ["gnome-extensions-app"] },
                        { Repository.RedHat, ["gnome-extensions-app"] },
                        { Repository.Ubuntu, ["gnome-shell-extensions"] },
                    },
                },
                new Package
                {
                    Name = "Gnome Shell Extension Manager",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Debian, ["gnome-shell-extension-manager"] },
                        { Repository.Ubuntu, ["gnome-shell-extension-manager"] },
                    },
                },
                new Package
                {
                    Name = "Gnome System Monitor",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-system-monitor"] },
                        { Repository.Debian, ["gnome-system-monitor"] },
                        { Repository.Fedora, ["gnome-system-monitor"] },
                        { Repository.RedHat, ["gnome-system-monitor"] },
                        { Repository.Ubuntu, ["gnome-system-monitor"] },
                    },
                },
                new Package
                {
                    Name = "Gnome Tweaks",
                    DesktopEnvironment = DesktopEnvironment.Gnome,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["gnome-tweaks"] },
                        { Repository.Debian, ["gnome-tweaks"] },
                        { Repository.Fedora, ["gnome-tweaks"] },
                        { Repository.RedHat, ["gnome-tweaks"] },
                        { Repository.Ubuntu, ["gnome-tweaks"] },
                    },
                },
                new Package
                {
                    Name = "KSysGuard",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["ksysguard"] },
                        { Repository.Debian, ["ksysguard"] },
                        { Repository.Fedora, ["ksysguard"] },
                        { Repository.RedHat, ["ksysguard"] },
                        { Repository.Ubuntu, ["ksysguard"] },
                    },
                },
                new Package
                {
                    Name = "Plasma System Monitor",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["plasma-systemmonitor"] },
                        { Repository.Debian, ["plasma-systemmonitor"] },
                        { Repository.Fedora, ["plasma-systemmonitor"] },
                        { Repository.RedHat, ["plasma-systemmonitor"] },
                        { Repository.Ubuntu, ["plasma-systemmonitor"] },
                    },
                },
                new Package
                {
                    Name = "Simple Scan",
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["simple-scan"] },
                        { Repository.Debian, ["simple-scan"] },
                        { Repository.Fedora, ["simple-scan"] },
                        { Repository.Ubuntu, ["simple-scan"] },
                    },
                },
                new Package
                {
                    Name = "Spectacle Screenshot",
                    DesktopEnvironment = DesktopEnvironment.Kde,
                    Repositories = new Dictionary<Repository, List<string>>()
                    {
                        { Repository.Arch, ["spectacle"] },
                        { Repository.Debian, ["spectacle"] },
                        { Repository.Fedora, ["spectacle"] },
                        { Repository.RedHat, ["spectacle"] },
                        { Repository.Ubuntu, ["spectacle"] },
                    },
                },
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };
    }
}