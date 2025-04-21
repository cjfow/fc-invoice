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
    // test anon method:
    // public string TestText => "Binding works!";

    public string? BillTo { get; } = invoice.BillTo;
    public string? ProjectNumber { get; } = invoice.ProjectNumber;
    public string? InvoiceNumber { get; } = invoice.InvoiceNumber;
    public DateTime SelectedDate { get; } = invoice.SelectedDate;
    public ObservableCollection<InvoiceItem> InvoiceItems { get; } = invoice.Items;
    public decimal Total { get; } = invoice.Total;

    [RelayCommand]
    public async Task PrintAndSaveAsync()
    {
        MessageBox.Show("Command reached.");
        foreach (Window window in Application.Current.Windows)
        {
            if (!window.Title.Contains("Invoice"))
                continue;

            if (window.FindName("PaperVisual") is not FrameworkElement element)
            {
                MessageBox.Show("PaperVisual not found.");
                return;
            }

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
            {
                MessageBox.Show("Print canceled.");
                break;
            }

            try
            {
                var capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
                var printableWidth = capabilities.PageImageableArea.ExtentWidth;
                var printableHeight = capabilities.PageImageableArea.ExtentHeight;

                // force layout and get desired size
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                element.Arrange(new Rect(new Point(0, 0), element.DesiredSize));
                element.UpdateLayout();

                // calculate scale
                double scale = Math.Min(
                    printableWidth / element.DesiredSize.Width,
                    printableHeight / element.DesiredSize.Height);

                // apply scale
                var originalTransform = element.LayoutTransform;
                element.LayoutTransform = new ScaleTransform(scale, scale);

                var scaledSize = new Size(element.DesiredSize.Width * scale, element.DesiredSize.Height * scale);
                element.Measure(scaledSize);
                element.Arrange(new Rect(new Point(0, 0), scaledSize));
                element.UpdateLayout();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    printDialog.PrintVisual(element, "Invoice Print");
                });

                element.LayoutTransform = originalTransform;

                MessageBox.Show($"Printing to: {printDialog.PrintQueue.Name}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing failed: {ex.Message}");
            }

            break;
        }

        // save the invoice after printing
        try
        {
            var jsonService = new JsonInvoiceStorageService();
            await jsonService.SaveInvoiceAsync(invoice);
            MessageBox.Show("Save complete!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
