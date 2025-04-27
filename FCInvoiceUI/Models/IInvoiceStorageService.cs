namespace FCInvoiceUI.Models;

interface IInvoiceStorageService
{
    Task SaveInvoiceAsync(BillingInvoice invoice);

    Task<BillingInvoice?> LoadInvoiceAsync(string invoiceNumber);
}