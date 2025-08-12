using FCInvoice.Core.Models;
using FCInvoice.UI.ViewModels;
using FCInvoiceTests.Mocks;

namespace FCInvoiceTests;

[TestClass]
public class PrintViewModelTests
{
    [TestMethod]
    public void Constructor_ShouldInitilizePropertiesCorrectly()
    {
        BillingInvoice invoice = new()
        {
            BillTo = "Test Customer",
            ProjectNumber = "12-345",
            InvoiceNumber = "1234567",
            SelectedDate = new DateTime(1300, 4, 30)
        };

        invoice.Items.Add(new InvoiceItem
        {
            Quantity = 22134,
            Description = "Test message",
            Rate = 10
        });

        var mockStorageService = new MockInvoiceStorageService();
        PrintViewModel viewModel = new(invoice, mockStorageService);

        Assert.AreEqual("Test Customer", viewModel.BillTo);
        Assert.AreEqual("12-345", viewModel.ProjectNumber);
        Assert.AreEqual("1234567", viewModel.InvoiceNumber);
        Assert.AreEqual(new DateTime(1300, 4, 30), viewModel.SelectedDate);
        Assert.AreEqual(invoice.Items.Count, viewModel.InvoiceItems.Count);
        Assert.AreEqual(invoice.Total, viewModel.Total);
    }
}
