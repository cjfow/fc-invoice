using FCInvoice.Core.Interfaces;
using FCInvoice.Core.Models;
using System.IO;
using System.Text.Json;

namespace FCInvoice.Core.Services;

/// <summary>
/// Service for storing and retrieving invoice data with AES encryption
/// </summary>
public class JsonInvoiceStorageService : IInvoiceStorageService
{
    private readonly string _baseDirectory;
    private readonly EncryptionService _encryptionService;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public JsonInvoiceStorageService()
    {
        _baseDirectory = Path.Combine(AppContext.BaseDirectory, "Resources", "Data");
        _encryptionService = new EncryptionService();
    }
    
    public JsonInvoiceStorageService(EncryptionService encryptionService)
    {
        _baseDirectory = Path.Combine(AppContext.BaseDirectory, "Resources", "Data");
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Saves an invoice to encrypted storage
    /// </summary>
    /// <param name="invoice">Invoice to save</param>
    /// <exception cref="ArgumentException">Thrown when invoice number is null or empty</exception>
    public async Task SaveInvoiceAsync(BillingInvoice invoice)
    {
        if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
            throw new ArgumentException("Invoice number must not be null or empty.", nameof(invoice));

        EnsureDirectoryExists();
        
        var tempJsonFile = GetTempFilePath(invoice.InvoiceNumber);
        var encryptedFile = GetEncryptedFilePath(invoice.InvoiceNumber);

        try
        {
            var json = JsonSerializer.Serialize(invoice, _jsonOptions);
            await File.WriteAllTextAsync(tempJsonFile, json);
            _encryptionService.EncryptFile(tempJsonFile, encryptedFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save invoice {invoice.InvoiceNumber}: {ex.Message}");
            throw;
        }
        finally
        {
            CleanupTempFile(tempJsonFile);
        }
    }

    /// <summary>
    /// Loads an invoice from encrypted storage
    /// </summary>
    /// <param name="invoiceNumber">Invoice number to load</param>
    /// <returns>Invoice if found, null otherwise</returns>
    public async Task<BillingInvoice?> LoadInvoiceAsync(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return null;
            
        var encryptedFile = GetEncryptedFilePath(invoiceNumber);
        var tempJsonFile = GetTempFilePath(invoiceNumber);

        if (!File.Exists(encryptedFile))
            return null;

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
            CleanupTempFile(tempJsonFile);
        }
    }

    /// <summary>
    /// Loads all invoices from encrypted storage, ordered by invoice number descending
    /// </summary>
    /// <returns>Collection of all invoices</returns>
    public async Task<IEnumerable<BillingInvoice>> LoadAllInvoicesAsync()
    {
        EnsureDirectoryExists();
        List<BillingInvoice> invoices = [];

        var encryptedFiles = Directory.GetFiles(_baseDirectory, "*.enc");
        
        foreach (var encryptedFile in encryptedFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(encryptedFile);
            if (fileName == null) continue;
            
            var invoice = await LoadSingleInvoiceFile(encryptedFile, fileName);
            if (invoice != null)
                invoices.Add(invoice);
        }

        return invoices.OrderByDescending(i => i.InvoiceNumber);
    }

    /// <summary>
    /// Deletes an invoice from storage
    /// </summary>
    /// <param name="invoiceNumber">Invoice number to delete</param>
    public async Task DeleteInvoiceAsync(string invoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            return;
            
        var fileName = GetEncryptedFilePath(invoiceNumber);
        if (File.Exists(fileName))
        {
            await Task.Run(() => File.Delete(fileName));
        }
    }
    
    /// <summary>
    /// Ensures the base directory exists for invoice storage
    /// </summary>
    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_baseDirectory))
            Directory.CreateDirectory(_baseDirectory);
    }
    
    /// <summary>
    /// Gets the file path for encrypted invoice files
    /// </summary>
    /// <param name="invoiceNumber">Invoice number</param>
    /// <returns>Full path to encrypted file</returns>
    private string GetEncryptedFilePath(string invoiceNumber) => 
        Path.Combine(_baseDirectory, $"{invoiceNumber}.enc");
    
    /// <summary>
    /// Gets the file path for temporary JSON files during encryption/decryption
    /// </summary>
    /// <param name="invoiceNumber">Invoice number</param>
    /// <returns>Full path to temporary file</returns>
    private string GetTempFilePath(string invoiceNumber) => 
        Path.Combine(_baseDirectory, $"{invoiceNumber}.tmp");
    
    /// <summary>
    /// Safely deletes a temporary file if it exists
    /// </summary>
    /// <param name="tempFilePath">Path to temporary file</param>
    private static void CleanupTempFile(string tempFilePath)
    {
        if (File.Exists(tempFilePath))
        {
            try
            {
                File.Delete(tempFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to cleanup temp file {tempFilePath}: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Loads a single invoice from an encrypted file
    /// </summary>
    /// <param name="encryptedFile">Path to encrypted file</param>
    /// <param name="fileName">Base filename without extension</param>
    /// <returns>Invoice if successfully loaded, null otherwise</returns>
    private async Task<BillingInvoice?> LoadSingleInvoiceFile(string encryptedFile, string fileName)
    {
        var tempJsonFile = GetTempFilePath(fileName);
        
        try
        {
            _encryptionService.DecryptFile(encryptedFile, tempJsonFile);
            var json = await File.ReadAllTextAsync(tempJsonFile);
            
            var invoice = JsonSerializer.Deserialize<BillingInvoice>(json);
            if (invoice == null)
            {
                Console.WriteLine($"File deserialized to null: {encryptedFile}");
            }
            
            return invoice;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load invoice from {encryptedFile}: {ex.Message}");
            return null;
        }
        finally
        {
            CleanupTempFile(tempJsonFile);
        }
    }
}