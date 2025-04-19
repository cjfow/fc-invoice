using System;
using System.IO;

namespace FCInvoiceUI.Services;

class InvoiceNumberGeneratorService
{
    private const string filePath = @"C:\Users\cfowl\source\repos\FCInvoice\FCInvoiceUI\Resources\Data\invoice_number.txt";

    public static string PeekNextInvoiceNumber()
    {
        int year = DateTime.Today.Year;
        int lastUsed = 0;
        int fileYear;

        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir) && !string.IsNullOrWhiteSpace(dir))
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

        return $"{year}{(lastUsed + 1):D3}";
    }

    public static void ReserveNextInvoiceNumber()
    {
        int year = DateTime.Today.Year;
        int lastUsed = 0;
        int fileYear;

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

        // TODO: Call this method when invoice is printed or saved
    }

    private static (int year, int count) ParseInvoiceData(string input)
    {
        try
        {
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
