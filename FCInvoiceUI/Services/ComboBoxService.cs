using FCInvoiceUI.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace FCInvoiceUI.Services;

public class ComboBoxFormatService
{
    private readonly string _invoicesFolderPath = @"C:\Users\cfowl\source\repos\FCInvoice\FCInvoiceUI\Resources\Data\";

    public ObservableCollection<BillingInvoice> LoadPreviousInvoices()
    {
        ObservableCollection<BillingInvoice> invoices = [];

        if (!Directory.Exists(_invoicesFolderPath))
        {
            return invoices;
        }

        string[] invoiceFiles = Directory.GetFiles(_invoicesFolderPath, "*.json");

        foreach (var file in invoiceFiles)
        {
            try
            {
                string json = File.ReadAllText(file);
                var invoice = JsonSerializer.Deserialize<BillingInvoice>(json);

                if (invoice is not null)
                {
                    invoices.Add(invoice);
                }
            }
            catch
            {
                Console.WriteLine("File unreadable");
                continue;
            }
        }
        return [..invoices.OrderByDescending(i => i.InvoiceNumber)];
    }
}