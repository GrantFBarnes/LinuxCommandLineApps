using System;
using System.Collections.Generic;
using Linux.Enums;

namespace Linux;

public sealed class Package
{
    public string Name = string.Empty;
    public DesktopEnvironment? DesktopEnvironment = null;
    public Dictionary<Repository, List<string>> Repositories = new();
    public Flatpak? Flatpak = null;
    public Snap? Snap = null;
    public Action<Distribution, InstallMethod>? PreInstall = null;
    public Action<Distribution, InstallMethod>? PostInstall = null;
}