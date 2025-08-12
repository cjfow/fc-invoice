using FCInvoice.Core.Models;
using FCInvoice.UI.ViewModels;
using FCInvoiceTests.TestHelpers;

namespace FCInvoiceTests;

[TestClass]
public class MainViewModelTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeInvoiceWithDefaults()
    {
        var viewModel = ViewModelTestHelper.CreateMainViewModelForTesting();

        Assert.IsTrue(viewModel.Invoice.IsCurrentInvoice);
        Assert.IsNotNull(viewModel.Invoice.InvoiceNumber);
        Assert.AreEqual(3, viewModel.InvoiceItems.Count);
        Assert.AreEqual(viewModel.Invoice, viewModel.SelectedInvoice);
        CollectionAssert.Contains(viewModel.FilteredInvoices.ToList(), viewModel.Invoice);
    }

    [TestMethod]
    public void BillTo_Setter_ShouldUpdateInvoiceBillTo()
    {
        var viewModel = ViewModelTestHelper.CreateMainViewModelForTesting();
        string newBillTo = "New Customer";

        viewModel.BillTo = newBillTo;

        Assert.AreEqual(newBillTo, viewModel.Invoice.BillTo);
    }

    [TestMethod]
    public void ProjectNumber_Setter_ShouldUpdateInvoiceProjectNumber()
    {
        var viewModel = ViewModelTestHelper.CreateMainViewModelForTesting();
        string newProjectNumber = "24-999";

        viewModel.ProjectNumber = newProjectNumber;

        Assert.AreEqual(newProjectNumber, viewModel.Invoice.ProjectNumber);
    }

    [TestMethod]
    public void SelectedDate_Setter_ShouldUpdateInvoiceSelectedDate()
    {
        var viewModel = ViewModelTestHelper.CreateMainViewModelForTesting();
        DateTime newDate = new(2025, 5, 1);

        viewModel.SelectedDate = newDate;

        Assert.AreEqual(newDate, viewModel.Invoice.SelectedDate);
    }

    [TestMethod]
    public void InvoiceNumber_Setter_ShouldUpdateInvoiceInvoiceNumber()
    {
        var viewModel = ViewModelTestHelper.CreateMainViewModelForTesting();
        string newInvoiceNumber = "2025002";

        viewModel.InvoiceNumber = newInvoiceNumber;

        Assert.AreEqual(newInvoiceNumber, viewModel.Invoice.InvoiceNumber);
    }

    [TestMethod]
    public void SelectingSameInvoice_ShouldRestoreOriginalInvoice()
    {
        var viewModel = ViewModelTestHelper.CreateMainViewModelForTesting();
        viewModel.BillTo = "Original BillTo";
        viewModel.CacheCurrentInvoice();

        viewModel.BillTo = "Modified BillTo";

        viewModel.SelectedInvoice = null;
        viewModel.SelectedInvoice = viewModel.FilteredInvoices.FirstOrDefault();

        Assert.AreEqual("Original BillTo", viewModel.BillTo);
    }


    [TestMethod]
    public void SelectingDifferentInvoice_ShouldCopyInvoice()
    {
        var viewModel = ViewModelTestHelper.CreateMainViewModelForTesting();
        BillingInvoice newInvoice = new()
        {
            BillTo = "Different Client",
            ProjectNumber = "24-888",
            SelectedDate = new DateTime(2025, 5, 2),
            InvoiceNumber = "2025005"
        };
        newInvoice.Items.Add(new InvoiceItem { Quantity = 2, Description = "Item X", Rate = 50m });
        viewModel.FilteredInvoices.Add(newInvoice);

        viewModel.SelectedInvoice = newInvoice;

        Assert.AreEqual("Different Client", viewModel.BillTo);
        Assert.AreEqual("24-888", viewModel.ProjectNumber);
        Assert.AreEqual(new DateTime(2025, 5, 2), viewModel.SelectedDate);
        Assert.IsTrue(viewModel.InvoiceItems.Any(i => i.Description == "Item X" && i.Rate == 50m));
    }
}
