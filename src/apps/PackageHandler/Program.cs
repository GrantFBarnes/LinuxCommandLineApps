using Linux;

namespace PackageHandler;

internal static class Program
{
    public static void Main()
    {
        var distribution = new Distribution();
        var mainMenu = new MainMenu(distribution);
        mainMenu.Run();
    }
}