using System;
using System.Collections.Generic;
using Linux.Enums;

namespace Linux;

public sealed class Package
{
    private readonly string _name = string.Empty;
    private readonly Dictionary<Repository, List<string>> _repositories = new();
    private readonly Flatpak? _flatpak = null;
    private readonly Snap? _snap = null;
    private readonly DesktopEnvironment? _desktopEnvironment = null;
    private readonly Action? _preInstall = null;
    private readonly Action? _postInstall = null;
}