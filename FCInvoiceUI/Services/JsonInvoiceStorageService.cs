using FCInvoiceUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FCInvoiceUI.Services;

public class JsonInvoiceStorageService : IInvoiceStorageService
{
    private readonly string _baseDirectory = @"C:\Users\cfowl\source\repos\FCInvoice\FCInvoiceUI\Resources\Data";
    private readonly EncryptionService _encryptionService = new();
    private readonly JsonSerializerOptions _cachedJsonSerializerOptions = new() { WriteIndented = true };

    public async Task SaveInvoiceAsync(BillingInvoice invoice)
    {
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
        {
            throw new ArgumentException("Invoice number must not be null or empty.", nameof(invoice));
        }

        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
        }

        string fileName = Path.Combine(_baseDirectory, $"{invoice.InvoiceNumber}.enc");

        string json = JsonSerializer.Serialize(invoice, _cachedJsonSerializerOptions);
        string encryptedJson = _encryptionService.Encrypt(json);
        await File.WriteAllTextAsync(fileName, encryptedJson);
    }

    public async Task<BillingInvoice?> LoadInvoiceAsync(string invoiceNumber)
    {
        string fileName = Path.Combine(_baseDirectory, $"{invoiceNumber}.enc");

        if (!File.Exists(fileName))
        {
            return null;
        }

        try
        {
            string encryptedJson = await File.ReadAllTextAsync(fileName);
            string decryptedJson = _encryptionService.Decrypt(encryptedJson);

            return JsonSerializer.Deserialize<BillingInvoice>(decryptedJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load invoice {invoiceNumber}: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<BillingInvoice>> LoadAllInvoicesAsync()
    {
        Directory.CreateDirectory(_baseDirectory);
        List<BillingInvoice> invoices = [];

        foreach (string file in Directory.GetFiles(_baseDirectory, "*.enc"))
        {
            try
            {
                string encryptedJson = await File.ReadAllTextAsync(file);
                string decryptedJson = _encryptionService.Decrypt(encryptedJson);

                if (JsonSerializer.Deserialize<BillingInvoice>(decryptedJson) is BillingInvoice invoice)
                {
                    invoices.Add(invoice);
                }
                else
                {
                    Console.WriteLine($"File deserialized to null: {file}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File unreadable: {file}");
                Console.WriteLine($"Reason: {ex.Message}");
            }
        }

        return invoices;
    }

    public async Task DeleteInvoiceAsync(string invoiceNumber)
    {
        string fileName = Path.Combine(_baseDirectory, $"{invoiceNumber}.enc");
        if (File.Exists(fileName))
        {
            await Task.Run(() => File.Delete(fileName));
        }
    }
}
