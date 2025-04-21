using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCInvoiceUI.Models;
using FCInvoiceUI.Services;

namespace FCInvoiceUI.ViewModels;

public partial class PrintViewModel(BillingInvoice invoice) : ObservableObject
{
    public string? BillTo { get; } = invoice.BillTo;
    public string? ProjectNumber { get; } = invoice.ProjectNumber;
    public string? InvoiceNumber { get; } = invoice.InvoiceNumber;
    public DateTime SelectedDate { get; } = invoice.SelectedDate;
    public ObservableCollection<InvoiceItem> InvoiceItems { get; } = invoice.Items;
    public decimal Total { get; } = invoice.Total;

    [RelayCommand]
    public async Task PrintAndSaveAsync()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window.Title != "Invoice Print Preview")
                continue;

            if (window.FindName("PaperVisual") is not FrameworkElement element)
                return;

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
                break;

            var capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);

            double scale = Math.Min(
                capabilities.PageImageableArea.ExtentWidth / element.ActualWidth,
                capabilities.PageImageableArea.ExtentHeight / element.ActualHeight);

            var originalTransform = element.LayoutTransform;
            element.LayoutTransform = new ScaleTransform(scale, scale);

            var size = new Size(element.ActualWidth, element.ActualHeight);
            element.Measure(size);
            element.Arrange(new Rect(new Point(0, 0), size));

            printDialog.PrintVisual(element, "Invoice Print");

            element.LayoutTransform = originalTransform;
            break;
        }

        // save the invoice after printing
        try
        {
            var jsonService = new JsonInvoiceStorageService();
            await jsonService.SaveInvoiceAsync(invoice);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
