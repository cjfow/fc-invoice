using FCInvoice.Core.Interfaces;
using FCInvoice.Core.Models;

namespace FCInvoiceTests.Mocks;

/// <summary>
/// Mock implementation of IInvoiceStorageService for testing
/// </summary>
public class MockInvoiceStorageService : IInvoiceStorageService
{
    private readonly Dictionary<string, BillingInvoice> _invoices = [];

    /// <summary>
    /// Saves an invoice to the in-memory collection
    /// </summary>
    /// <param name="invoice">Invoice to save</param>
    public Task SaveInvoiceAsync(BillingInvoice invoice)
    {
        if (invoice.InvoiceNumber != null)
        {
            _invoices[invoice.InvoiceNumber] = invoice;
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Loads an invoice from the in-memory collection
    /// </summary>
    /// <param name="invoiceNumber">Invoice number to load</param>
    /// <returns>Invoice if found, null otherwise</returns>
    public Task<BillingInvoice?> LoadInvoiceAsync(string invoiceNumber)
    {
        _invoices.TryGetValue(invoiceNumber, out var invoice);
        return Task.FromResult(invoice);
    }

    /// <summary>
    /// Loads all invoices from the in-memory collection
    /// </summary>
    /// <returns>Collection of all invoices</returns>
    public Task<IEnumerable<BillingInvoice>> LoadAllInvoicesAsync()
    {
        var invoices = _invoices.Values.OrderByDescending(i => i.InvoiceNumber);
        return Task.FromResult<IEnumerable<BillingInvoice>>(invoices);
    }

    /// <summary>
    /// Deletes an invoice from the in-memory collection
    /// </summary>
    /// <param name="invoiceNumber">Invoice number to delete</param>
    public Task DeleteInvoiceAsync(string invoiceNumber)
    {
        _invoices.Remove(invoiceNumber);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds a test invoice to the collection
    /// </summary>
    /// <param name="invoice">Invoice to add</param>
    public void AddTestInvoice(BillingInvoice invoice)
    {
        if (invoice.InvoiceNumber != null)
        {
            _invoices[invoice.InvoiceNumber] = invoice;
        }
    }

    /// <summary>
    /// Clears all invoices from the collection
    /// </summary>
    public void Clear()
    {
        _invoices.Clear();
    }

    /// <summary>
    /// Gets the number of invoices in the collection
    /// </summary>
    public int Count => _invoices.Count;
}