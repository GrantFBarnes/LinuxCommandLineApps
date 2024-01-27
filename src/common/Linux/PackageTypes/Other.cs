using System;
using System.Collections.Generic;
using System.Linq;
using Linux.Enums;

namespace Linux.PackageTypes;

public sealed class Other(OtherPackage package)
{
    public readonly OtherPackage Package = package;

    public static List<OtherPackage> GetInstalled()
    {
        var packages = new List<OtherPackage>();

        foreach (var package in Enum.GetValues(typeof(OtherPackage)).Cast<OtherPackage>())
        {
            switch (package)
            {
                case OtherPackage.Rust:
                    if (new Command("rustup").Exists())
                    {
                        packages.Add(package);
                    }

                    break;
                default:
                    throw new Exception("package get installed not handled");
            }
        }

        return packages;
    }

    public static void Update()
    {
        foreach (var package in Enum.GetValues(typeof(OtherPackage)).Cast<OtherPackage>())
        {
            switch (package)
            {
                case OtherPackage.Rust:
                    if (new Command("rustup").Exists())
                    {
                        new Command("rustup self update").Run();
                        new Command("rustup update stable").Run();
                    }

                    break;
                default:
                    throw new Exception("package update not handled");
            }
        }
    }

    public void Install(Distribution distribution)
    {
        if (distribution.InstalledOther.Contains(Package)) return;
        distribution.InstalledOther.Add(Package);

        switch (Package)
        {
            case OtherPackage.Rust:
                new Command("curl '=https' --tlsv1.2 -sSf https://sh.rustup.rs").PipeInto("sh");
                break;
            default:
                throw new Exception("package update not handled");
        }
    }

    public void UnInstall(Distribution distribution)
    {
        if (!distribution.InstalledOther.Contains(Package)) return;
        distribution.InstalledOther.Remove(Package);

        switch (Package)
        {
            case OtherPackage.Rust:
                if (new Command("rustup").Exists())
                {
                    new Command("rustup self uninstall").Run();
                }

                break;
            default:
                throw new Exception("package update not handled");
        }
    }
}