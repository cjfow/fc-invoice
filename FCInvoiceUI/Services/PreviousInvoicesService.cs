using FCInvoiceUI.Models;
using System.IO;
using System.Text.Json;

namespace FCInvoiceUI.Services;

class PreviousInvoicesService
{
    private readonly string _invoicesFolderPath;
    private readonly string _errorLogPath;
    private readonly EncryptionService _encryptionService = new();

    public PreviousInvoicesService()
    {
        var basePath = Path.Combine(AppContext.BaseDirectory, "Resources", "Data");
        _invoicesFolderPath = basePath;
        _errorLogPath = Path.Combine(basePath, "InvoiceLoadErrors.txt");
    }

    public IEnumerable<BillingInvoice> LoadAllPreviousInvoices()
    {
        if (!Directory.Exists(_invoicesFolderPath))
        {
            return [];
        }

        var invoiceFiles = Directory.GetFiles(_invoicesFolderPath, "*.enc");
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

        var filePath = Path.Combine(_invoicesFolderPath, $"{invoiceNumber}.enc");

        if (!File.Exists(filePath))
        {
            return null;
        }

        return LoadInvoiceFromFile(filePath);
    }

    public string GetNextInvoiceNumber()
    {
        var currentYear = DateTime.Today.Year;

        if (!Directory.Exists(_invoicesFolderPath))
        {
            Directory.CreateDirectory(_invoicesFolderPath);
        }

        var invoiceFiles = Directory.GetFiles(_invoicesFolderPath, "*.enc");
        var validCounts = new List<int>();

        foreach (var file in invoiceFiles)
        {
            var name = Path.GetFileNameWithoutExtension(file);

            if (IsValidInvoiceNumber(name))
            {
                int count = int.Parse(name![4..]);
                validCounts.Add(count);
            }
        }

        var nextCount = validCounts.Count != 0 ? validCounts.Max() + 1 : 1;

        return $"{currentYear}{nextCount:000}";
    }

    private BillingInvoice? LoadInvoiceFromFile(string filePath)
    {
        var tempJsonFile = Path.ChangeExtension(filePath, ".tmp");

        try
        {
            _encryptionService.DecryptFile(filePath, tempJsonFile);
            var json = File.ReadAllText(tempJsonFile);

            var invoice = JsonSerializer.Deserialize<BillingInvoice>(json);

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
        finally
        {
            if (File.Exists(tempJsonFile))
            {
                File.Delete(tempJsonFile);
            }
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
            var logEntry = $"{DateTime.Now:g} - {message}{Environment.NewLine}";
            File.AppendAllText(_errorLogPath, logEntry);
        }
        catch
        {
            Console.WriteLine($"Logging to file failed: {message}");
        }
    }
}
