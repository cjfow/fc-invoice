using FCInvoice.Core.Interfaces;
using FCInvoice.Core.Models;
using FCInvoice.Core.Services;
using System.Collections.ObjectModel;

namespace FCInvoice.UI.Services;

/// <summary>
/// Service for loading invoices formatted for ComboBox display
/// </summary>
public class ComboBoxFormatService(IInvoiceStorageService storageService)
{
    private readonly IInvoiceStorageService _storageService = storageService;

    public ComboBoxFormatService() : this(new JsonInvoiceStorageService()) { }

    /// <summary>
    /// Loads all previous invoices for ComboBox display
    /// </summary>
    /// <returns>Observable collection of invoices ordered by invoice number descending</returns>
    /// 
    public virtual async Task<ObservableCollection<BillingInvoice>> LoadPreviousInvoicesAsync()
    {
        try
        {
            var invoices = await _storageService.LoadAllInvoicesAsync();
            return new ObservableCollection<BillingInvoice>(invoices);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load previous invoices: {ex.Message}");
            return [];
        }
    }
    
    /// <summary>
    /// Synchronous version for backward compatibility - prefer async version
    /// </summary>
    /// <returns>Observable collection of invoices</returns>
    public virtual ObservableCollection<BillingInvoice> LoadPreviousInvoices() =>
        LoadPreviousInvoicesAsync().GetAwaiter().GetResult();
}
