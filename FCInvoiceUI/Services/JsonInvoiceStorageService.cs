﻿using FCInvoiceUI.Models;
using System.IO;
using System.Text.Json;

namespace FCInvoiceUI.Services;

public class JsonInvoiceStorageService : IInvoiceStorageService
{
    private readonly string _baseDirectory;
    private readonly EncryptionService _encryptionService = new();
    private readonly JsonSerializerOptions _cachedJsonSerializerOptions = new() { WriteIndented = true };

    public JsonInvoiceStorageService()
    {
        _baseDirectory = Path.Combine(AppContext.BaseDirectory, "Resources", "Data");
    }

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

        var tempJsonFile = Path.Combine(_baseDirectory, $"{invoice.InvoiceNumber}.tmp");
        var encryptedFile = Path.Combine(_baseDirectory, $"{invoice.InvoiceNumber}.enc");

        try
        {
            var json = JsonSerializer.Serialize(invoice, _cachedJsonSerializerOptions);
            await File.WriteAllTextAsync(tempJsonFile, json);
            _encryptionService.EncryptFile(tempJsonFile, encryptedFile);
        }
        finally
        {
            if (File.Exists(tempJsonFile))
            {
                File.Delete(tempJsonFile);
            }
        }
    }

    public async Task<BillingInvoice?> LoadInvoiceAsync(string invoiceNumber)
    {
        var encryptedFile = Path.Combine(_baseDirectory, $"{invoiceNumber}.enc");
        var tempJsonFile = Path.Combine(_baseDirectory, $"{invoiceNumber}.tmp");

        if (!File.Exists(encryptedFile))
        {
            return null;
        }

        try
        {
            _encryptionService.DecryptFile(encryptedFile, tempJsonFile);
            var json = await File.ReadAllTextAsync(tempJsonFile);
            return JsonSerializer.Deserialize<BillingInvoice>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load invoice {invoiceNumber}: {ex.Message}");
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

    public async Task<IEnumerable<BillingInvoice>> LoadAllInvoicesAsync()
    {
        Directory.CreateDirectory(_baseDirectory);
        List<BillingInvoice> invoices = [];

        foreach (var encryptedFile in Directory.GetFiles(_baseDirectory, "*.enc"))
        {
            var fileName = Path.GetFileNameWithoutExtension(encryptedFile);
            var tempJsonFile = Path.Combine(_baseDirectory, $"{fileName}.tmp");

            try
            {
                _encryptionService.DecryptFile(encryptedFile, tempJsonFile);
                var json = await File.ReadAllTextAsync(tempJsonFile);

                if (JsonSerializer.Deserialize<BillingInvoice>(json) is BillingInvoice invoice)
                {
                    invoices.Add(invoice);
                }
                else
                {
                    Console.WriteLine($"File deserialized to null: {encryptedFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"File unreadable: {encryptedFile}");
                Console.WriteLine($"Reason: {ex.Message}");
            }
            finally
            {
                if (File.Exists(tempJsonFile))
                {
                    File.Delete(tempJsonFile);
                }
            }
        }

        return invoices.OrderByDescending(i => i.InvoiceNumber);
    }

    public async Task DeleteInvoiceAsync(string invoiceNumber)
    {
        var fileName = Path.Combine(_baseDirectory, $"{invoiceNumber}.enc");
        if (File.Exists(fileName))
        {
            await Task.Run(() => File.Delete(fileName));
        }
    }
}
