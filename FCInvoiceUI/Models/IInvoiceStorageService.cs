
namespace FCInvoiceUI.Models;

interface IInvoiceStorageService
{
    // save a single invoice asynchronously
    Task SaveInvoiceAsync(BillingInvoice invoice);

    // load a specific invoice by number
    Task<BillingInvoice?> LoadInvoiceAsync(string invoiceNumber);

    // load all saved invoices
    Task<IEnumerable<BillingInvoice>> LoadAllInvoicesAsync();

    // delete an invoice
    Task DeleteInvoiceAsync(string invoiceNumber);
}
