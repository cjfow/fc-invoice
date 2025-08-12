using FCInvoice.Core.Models;
using FCInvoice.UI.Services;
using System.Collections.ObjectModel;

namespace FCInvoiceTests.Mocks;

/// <summary>
/// Mock implementation of ComboBoxFormatService for testing
/// </summary>
public class MockComboBoxService : ComboBoxFormatService
{
    private readonly List<BillingInvoice> _testInvoices = [];

    public MockComboBoxService() : base(new MockInvoiceStorageService()) { }

    /// <summary>
    /// Loads previous invoices from the test collection
    /// </summary>
    /// <returns>Observable collection of test invoices</returns>
    public override ObservableCollection<BillingInvoice> LoadPreviousInvoices()
    {
        return new ObservableCollection<BillingInvoice>(_testInvoices.OrderByDescending(i => i.InvoiceNumber));
    }

    /// <summary>
    /// Adds a test invoice to the collection
    /// </summary>
    /// <param name="invoice">Invoice to add</param>
    public void AddTestInvoice(BillingInvoice invoice)
    {
        _testInvoices.Add(invoice);
    }

    /// <summary>
    /// Clears all test invoices
    /// </summary>
    public void ClearTestInvoices()
    {
        _testInvoices.Clear();
    }
}