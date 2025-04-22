
namespace FCInvoiceUI.Models;

interface IInvoiceStorageService
{
    Task SaveInvoiceAsync(BillingInvoice invoice);

    Task<BillingInvoice?> LoadInvoiceAsync(string invoiceNumber);

    Task<IEnumerable<BillingInvoice>> LoadAllInvoicesAsync();

    Task DeleteInvoiceAsync(string invoiceNumber);
}
