using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FCInvoiceUI.Services;

class InvoiceNumberGeneratorService
{
    private const string folderPath = @"C:\Users\cfowl\source\repos\FCInvoice\FCInvoiceUI\Resources\Data";

    public static string GetNextInvoiceNumber()
    {
        int currentYear = DateTime.Today.Year;

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            return $"{currentYear}001";
        }

        // get all the current files in the data folder, remove .json ext,
        // make sure they fit the yyyyxxx format, select confirms elements are safe to deref, make them a list
        var invoiceFiles = Directory.GetFiles(folderPath, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => name is string n && n.Length == 7 && n.StartsWith(currentYear.ToString()))
            .Select(name => name!)
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
