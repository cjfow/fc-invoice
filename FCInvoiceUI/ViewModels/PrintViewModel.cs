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
        foreach (Window window in Application.Current.Windows)
        {
            if (!window.Title.Equals("Invoice Print Preview", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (LogicalTreeHelper.FindLogicalNode(window, "PaperVisual") is not FrameworkElement paperVisual)
            {
                MessageBox.Show("Print preview not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
            {
                MessageBox.Show("Print canceled.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                break;
            }

            try
            {
                var capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
                var printableWidth = capabilities.PageImageableArea.ExtentWidth;
                var printableHeight = capabilities.PageImageableArea.ExtentHeight;

                paperVisual.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                paperVisual.Arrange(new Rect(new Point(0, 0), paperVisual.DesiredSize));
                paperVisual.UpdateLayout();

                var scale = Math.Min(printableWidth / paperVisual.DesiredSize.Width, printableHeight / paperVisual.DesiredSize.Height);

                var originalTransform = paperVisual.LayoutTransform;
                paperVisual.LayoutTransform = new ScaleTransform(scale, scale);

                var scaledSize = new Size(paperVisual.DesiredSize.Width * scale, paperVisual.DesiredSize.Height * scale);
                paperVisual.Measure(scaledSize);
                paperVisual.Arrange(new Rect(new Point(0, 0), scaledSize));
                paperVisual.UpdateLayout();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    printDialog.PrintVisual(paperVisual, "Invoice Print");
                });

                paperVisual.LayoutTransform = originalTransform;

                MessageBox.Show($"Printed to: {printDialog.PrintQueue.Name}", "Print Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            break;
        }

        try
        {
            var jsonService = new JsonInvoiceStorageService();
            await jsonService.SaveInvoiceAsync(invoice);
            MessageBox.Show("Save complete!", "Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save invoice: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
