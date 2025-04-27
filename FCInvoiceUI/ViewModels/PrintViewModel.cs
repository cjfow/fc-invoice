using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCInvoiceUI.Models;
using FCInvoiceUI.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        // TESTING: MessageBox.Show("Command reached.");

        foreach (Window window in Application.Current.Windows)
        {
            if (!window.Title.Contains("Invoice"))
            {
                continue;
            }

            if (window.FindName("PaperVisual") is not FrameworkElement element)
            {
                MessageBox.Show("Print preview not found :(");
                return;
            }

            PrintDialog printDialog = new();

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

                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                element.Arrange(new Rect(new Point(0, 0), element.DesiredSize));
                element.UpdateLayout();

                double scale = Math.Min(printableWidth / element.DesiredSize.Width, printableHeight / element.DesiredSize.Height);

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

        try
        {
            JsonInvoiceStorageService jsonService = new();
            await jsonService.SaveInvoiceAsync(invoice);
            MessageBox.Show("Save complete!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
