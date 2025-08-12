using FCInvoice.Core.Models;

namespace FCInvoice.Core.Interfaces;

/// <summary>
/// Interface for invoice storage operations
/// </summary>
public interface IInvoiceStorageService
{
    /// <summary>
    /// Saves an invoice to storage
    /// </summary>
    /// <param name="invoice">Invoice to save</param>
    Task SaveInvoiceAsync(BillingInvoice invoice);

    /// <summary>
    /// Loads an invoice from storage by its number
    /// </summary>
    /// <param name="invoiceNumber">Invoice number to load</param>
    /// <returns>Invoice if found, null otherwise</returns>
    Task<BillingInvoice?> LoadInvoiceAsync(string invoiceNumber);
    
    /// <summary>
    /// Loads all invoices from storage
    /// </summary>
    /// <returns>Collection of all invoices</returns>
    Task<IEnumerable<BillingInvoice>> LoadAllInvoicesAsync();
    
    /// <summary>
    /// Deletes an invoice from storage
    /// </summary>
    /// <param name="invoiceNumber">Invoice number to delete</param>
    Task DeleteInvoiceAsync(string invoiceNumber);
}