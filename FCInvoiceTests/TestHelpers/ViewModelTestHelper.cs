using FCInvoice.Core.Models;
using FCInvoice.UI.ViewModels;
using FCInvoiceTests.Mocks;

namespace FCInvoiceTests.TestHelpers;

/// <summary>
/// Helper class for creating view models with test dependencies
/// </summary>
public static class ViewModelTestHelper
{
    /// <summary>
    /// Creates a MainViewModel with mock dependencies for testing
    /// </summary>
    /// <returns>MainViewModel with mock dependencies</returns>
    public static MainViewModel CreateMainViewModelForTesting()
    {
        var mockComboBoxService = new MockComboBoxService();
        var mockInvoiceNumberGenerator = new MockInvoiceNumberGenerator();
        return new MainViewModel(mockComboBoxService, mockInvoiceNumberGenerator);
    }

    /// <summary>
    /// Creates a MainViewModel with pre-loaded test data
    /// </summary>
    /// <returns>MainViewModel with test invoices</returns>
    public static MainViewModel CreateMainViewModelWithTestData()
    {
        var mockComboBoxService = new MockComboBoxService();
        var mockInvoiceNumberGenerator = new MockInvoiceNumberGenerator();
        
        // Add some test invoices
        mockComboBoxService.AddTestInvoice(new BillingInvoice
        {
            InvoiceNumber = "2024001",
            BillTo = "Test Client 1",
            ProjectNumber = "24-001",
            SelectedDate = new DateTime(2024, 1, 15)
        });

        mockComboBoxService.AddTestInvoice(new BillingInvoice
        {
            InvoiceNumber = "2024002", 
            BillTo = "Test Client 2",
            ProjectNumber = "24-002",
            SelectedDate = new DateTime(2024, 2, 15)
        });

        return new MainViewModel(mockComboBoxService, mockInvoiceNumberGenerator);
    }
}