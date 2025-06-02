using FCInvoiceUI.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace FCInvoiceUI.Services;

public class ComboBoxFormatService
{
    private readonly string _invoicesFolderPath;
    private readonly EncryptionService _encryptionService = new();

    public ComboBoxFormatService()
    {
        _invoicesFolderPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Data");
    }

    public ObservableCollection<BillingInvoice> LoadPreviousInvoices()
    {
        ObservableCollection<BillingInvoice> invoices = [];

        if (!Directory.Exists(_invoicesFolderPath))
        {
            return invoices;
        }

        string[] invoiceFiles = Directory.GetFiles(_invoicesFolderPath, "*.enc");

        foreach (var file in invoiceFiles)
        {
            string tempJsonFile = Path.ChangeExtension(file, ".tmp");

            try
            {
                _encryptionService.DecryptFile(file, tempJsonFile);

                string json = File.ReadAllText(tempJsonFile);
                var invoice = JsonSerializer.Deserialize<BillingInvoice>(json);

                if (invoice is not null)
                {
                    invoices.Add(invoice);
                }
            }
            catch
            {
                Console.WriteLine($"File unreadable: {file}");
            }
            finally
            {
                if (File.Exists(tempJsonFile))
                {
                    File.Delete(tempJsonFile);
                }
            }
        }

        return [.. invoices.OrderByDescending(i => i.InvoiceNumber)];
    }
}
