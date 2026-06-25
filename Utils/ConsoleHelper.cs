namespace SIMSProject.Utils;

public static class ConsoleHelper
{
    public static void PrintHeader(string title)
    {
        try
        {
            Console.Clear();
        }
        catch (Exception)
        {
        }

        Console.WriteLine(new string('=', title.Length));
        Console.WriteLine(title);
        Console.WriteLine(new string('=', title.Length));
        Console.WriteLine();
    }

    public static void PrintSuccess(string message)
    {
        Console.WriteLine($"[OK] {message}");
    }

    public static void PrintError(string message)
    {
        Console.WriteLine($"[GRESKA] {message}");
    }

    public static void Pause()
    {
        Console.WriteLine();
        Console.Write("Pritisnite Enter za nastavak...");
        Console.ReadLine();
    }
}
