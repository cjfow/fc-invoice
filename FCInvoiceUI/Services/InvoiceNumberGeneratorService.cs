
using CommunityToolkit.Diagnostics;
using System.IO;

namespace FCInvoiceUI.Services;

class InvoiceNumberGeneratorService
{
    private const string filePath = "Resources/Data/invoice_number.txt";

    public static uint GetNextInvoiceNumber()
    {
        int year = DateTime.Today.Year;
        int lastUsed = 0;
        int fileYear;

        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (File.Exists(filePath))
        {
            var content = File.ReadAllText(filePath);
            (fileYear, lastUsed) = ParseInvoiceData(content);
        }
        else
        {
            fileYear = year;
        }

        if (fileYear != year)
        {
            lastUsed = 0;
        }

        if (lastUsed >= 999)
        {
            throw new InvalidOperationException("Maximum number of invoices reached for the year.");
        }

        lastUsed++;

        File.WriteAllText(filePath, $"{year}-{lastUsed}");

        return uint.Parse($"{year}{lastUsed:D3}");
    }

    private static (int year, int count) ParseInvoiceData(string input)
    {
        try
        {
            // try to split the input string by '-' into two parts using a tuple
            // if its  successful store them as yearStr and countStr
            // if the format is wrong or splitting fails default both to "0"
            (string yearStr, string countStr) = input.Split('-') switch
            {
                [var y, var c] => (y, c),
                _ => ("0", "0")
            };


            return (int.Parse(yearStr), int.Parse(countStr));
        }
        catch
        {
            return (DateTime.Today.Year, 0);
        }
    }
}
