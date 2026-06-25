namespace SIMSProject.Utils;

public static class InputValidator
{
    public static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            ConsoleHelper.PrintError("Unos je obavezan.");
        }
    }

    public static string? ReadOptionalString(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim();
    }

    public static int ReadInt(string prompt, int? minValue = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var raw = Console.ReadLine()?.Trim();
            if (int.TryParse(raw, out var value) && (!minValue.HasValue || value >= minValue.Value))
            {
                return value;
            }

            ConsoleHelper.PrintError("Unesite ispravan broj.");
        }
    }

    public static int ReadMenuChoice(string prompt, params int[] allowedValues)
    {
        while (true)
        {
            var choice = ReadInt(prompt);
            if (allowedValues.Contains(choice))
            {
                return choice;
            }

            ConsoleHelper.PrintError("Izabrana opcija nije dozvoljena.");
        }
    }

    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var value = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (value is "y" or "yes" or "da")
            {
                return true;
            }

            if (value is "n" or "no" or "ne")
            {
                return false;
            }

            ConsoleHelper.PrintError("Unesite y/n.");
        }
    }
}
