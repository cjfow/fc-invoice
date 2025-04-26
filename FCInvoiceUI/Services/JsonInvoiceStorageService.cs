using FCInvoiceUI.Models;
using System.IO;
using System.Text.Json;

namespace FCInvoiceUI.Services;

public class JsonInvoiceStorageService : IInvoiceStorageService
{
    private readonly string _baseDirectory = @"C:\Users\cfowl\source\repos\FCInvoice\FCInvoiceUI\Resources\Data";

    public async Task SaveInvoiceAsync(BillingInvoice invoice)
    {
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            ArgumentException argumentException = new("Invoice number must not be null or empty.", nameof(invoice));
            throw argumentException;
        }

        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
        }

        string fileName = Path.Combine(_baseDirectory, $"{invoice.InvoiceNumber}.json");

        JsonSerializerOptions jsonSerializerOptions = new() { WriteIndented = true };
        JsonSerializerOptions options = jsonSerializerOptions;
        string json = JsonSerializer.Serialize(invoice, options);

        await File.WriteAllTextAsync(fileName, json);
    }


    public async Task<BillingInvoice?> LoadInvoiceAsync(string invoiceNumber)
    {
        string fileName = Path.Combine(_baseDirectory, $"{invoiceNumber}.json");

        if (!File.Exists(fileName))
            return null;

        string json = await File.ReadAllTextAsync(fileName);
        return JsonSerializer.Deserialize<BillingInvoice>(json);
    }

    public async Task<IEnumerable<BillingInvoice>> LoadAllInvoicesAsync()
    {
        Directory.CreateDirectory(_baseDirectory);
        var invoices = new List<BillingInvoice>();

        foreach (string file in Directory.GetFiles(_baseDirectory, "*.json"))
        {
            string json = await File.ReadAllTextAsync(file);
            if (JsonSerializer.Deserialize<BillingInvoice>(json) is BillingInvoice invoice)
            {
                invoices.Add(invoice);
            }
        }

        return invoices;
    }

    public async Task DeleteInvoiceAsync(string invoiceNumber)
    {
        string fileName = Path.Combine(_baseDirectory, $"{invoiceNumber}.json");
        if (File.Exists(fileName))
        {
            await Task.Run(() => File.Delete(fileName));
        }
    }
}
