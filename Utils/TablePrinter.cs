namespace SIMSProject.Utils;

public static class TablePrinter
{
    public static void Print(string[] headers, List<string[]> rows)
    {
        if (headers.Length == 0)
        {
            return;
        }

        var widths = new int[headers.Length];
        for (var index = 0; index < headers.Length; index++)
        {
            widths[index] = headers[index].Length;
        }

        foreach (var row in rows)
        {
            for (var index = 0; index < headers.Length; index++)
            {
                var value = index < row.Length ? row[index] : string.Empty;
                widths[index] = Math.Max(widths[index], value.Length);
            }
        }

        PrintRow(headers, widths);
        PrintSeparator(widths);

        foreach (var row in rows)
        {
            PrintRow(row, widths);
        }
    }

    private static void PrintRow(IReadOnlyList<string> values, IReadOnlyList<int> widths)
    {
        for (var index = 0; index < widths.Count; index++)
        {
            var value = index < values.Count ? values[index] : string.Empty;
            Console.Write($"| {value.PadRight(widths[index])} ");
        }

        Console.WriteLine("|");
    }

    private static void PrintSeparator(IEnumerable<int> widths)
    {
        foreach (var width in widths)
        {
            Console.Write($"+-{new string('-', width)}-");
        }

        Console.WriteLine("+");
    }
}
