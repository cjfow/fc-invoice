using System.IO;

namespace FCInvoiceUI.Services;

class InvoiceNumberGeneratorService
{
    private static readonly string s_folderPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Data");

    public static string GetNextInvoiceNumber()
    {
        int currentYear = DateTime.Today.Year;

        if (!Directory.Exists(s_folderPath))
        {
            Directory.CreateDirectory(s_folderPath);
            return $"{currentYear}001";
        }

        // get all the current files in the data folder, remove .enc ext,
        // gets type of string as a null check, make sure they fit the yyyyxxx format, make them a list
        var invoiceFiles = Directory.GetFiles(s_folderPath, "*.enc")
            .Select(Path.GetFileNameWithoutExtension)
            .OfType<string>()
            .Where(name => name is not null && name.Length == 7 && name.StartsWith(currentYear.ToString()))
            .ToList();

        List<int> invoiceNumbers = [];

        foreach (var name in invoiceFiles)
        {
            string numberPart = name[4..];
            if (int.TryParse(numberPart, out int number))
            {
                invoiceNumbers.Add(number);
            }
        }

        int nextNumber = (invoiceNumbers.Count != 0 ? invoiceNumbers.Max() : 0) + 1;
        return $"{currentYear}{nextNumber:D3}";
    }
}
