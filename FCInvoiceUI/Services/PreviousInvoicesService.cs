using FCInvoiceUI.Models;
using System.IO;
using System.Text.Json;

namespace FCInvoiceUI.Services;

class PreviousInvoicesService
{
    private readonly string _invoicesFolderPath = @"C:\Users\cfowl\source\repos\FCInvoice\FCInvoiceUI\Resources\Data\";
    private readonly string _errorLogPath = @"C:\Users\cfowl\source\repos\FCInvoice\FCInvoiceUI\Resources\InvoiceLoadErrors.txt";

    public IEnumerable<BillingInvoice> LoadAllPreviousInvoices()
    {
        if (!Directory.Exists(_invoicesFolderPath))
        {
            return [];
        }

        string[] invoiceFiles = Directory.GetFiles(_invoicesFolderPath, "*.json");

        List<BillingInvoice> validInvoices = [];

        foreach (var file in invoiceFiles)
        {
            BillingInvoice? invoice = LoadInvoiceFromFile(file);

            if (invoice is not null)
            {
                validInvoices.Add(invoice);
            }
        }

        return validInvoices.OrderByDescending(invoice => invoice.InvoiceNumber);
    }

    public BillingInvoice? LoadInvoice(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber) || invoiceNumber.Length != 7)
        {
            return null;
        }

        string filePath = Path.Combine(_invoicesFolderPath, $"{invoiceNumber}.json");

        if (!File.Exists(filePath))
        {
            return null;
        }

        return LoadInvoiceFromFile(filePath);
    }

    public string GetNextInvoiceNumber()
    {
        int currentYear = DateTime.Today.Year;

        if (!Directory.Exists(_invoicesFolderPath))
        {
            Directory.CreateDirectory(_invoicesFolderPath);
        }

        string[] invoiceFiles = Directory.GetFiles(_invoicesFolderPath, "*.json");

        List<int> validCounts = [];

        foreach (var file in invoiceFiles)
        {
            string name = Path.GetFileNameWithoutExtension(file);

            if (IsValidInvoiceNumber(name))
            {
                int count = int.Parse(name![4..]);
                validCounts.Add(count);
            }
        }

        int nextCount = validCounts.Count != 0 ? validCounts.Max() + 1 : 1;

        return $"{currentYear}{nextCount:000}";
    }

    private BillingInvoice? LoadInvoiceFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            string jsonContent = File.ReadAllText(filePath);

            var invoice = JsonSerializer.Deserialize<BillingInvoice>(jsonContent);

            if (invoice is null)
            {
                LogError($"Deserialize() returned null for: {filePath}");
            }

            return invoice;
        }
        catch (Exception ex)
        {
            LogError($"Failed to load invoice from: {filePath}. Error: {ex.Message}");
            return null;
        }
    }

    private static bool IsValidInvoiceNumber(string? invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber) || invoiceNumber.Length != 7)
        {
            return false;
        }

        return int.TryParse(invoiceNumber.AsSpan(0, 4), out _) &&
               int.TryParse(invoiceNumber.AsSpan(4), out _);
    }

    private void LogError(string message)
    {
        try
        {
            string logEntry = $"{DateTime.Now:g} - {message}{Environment.NewLine}";
            File.AppendAllText(_errorLogPath, logEntry);
        }
        catch
        {
            Console.WriteLine($"Logging to file failed: {message}");
        }
    }
}
