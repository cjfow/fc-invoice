using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCInvoice.Core.Interfaces;
using FCInvoice.Core.Models;
using FCInvoice.Core.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FCInvoice.UI.ViewModels;

/// <summary>
/// View model for the invoice print preview functionality
/// </summary>
public partial class PrintViewModel : ObservableObject
{
    private readonly BillingInvoice _invoice;
    private readonly IInvoiceStorageService _storageService;
    
    public PrintViewModel(BillingInvoice invoice) : this(invoice, new JsonInvoiceStorageService()) { }
    
    public PrintViewModel(BillingInvoice invoice, IInvoiceStorageService storageService)
    {
        _invoice = invoice;
        _storageService = storageService;
    }
    
    /// <summary>
    /// Customer/company name to bill for this invoice
    /// </summary>
    public string? BillTo => _invoice.BillTo;
    
    /// <summary>
    /// Project number associated with this invoice
    /// </summary>
    public string? ProjectNumber => _invoice.ProjectNumber;
    
    /// <summary>
    /// Unique invoice number in YYYYNNN format
    /// </summary>
    public string? InvoiceNumber => _invoice.InvoiceNumber;
    
    /// <summary>
    /// Date for this invoice
    /// </summary>
    public DateTime SelectedDate => _invoice.SelectedDate;
    
    /// <summary>
    /// Collection of line items on this invoice
    /// </summary>
    public ObservableCollection<InvoiceItem> InvoiceItems => _invoice.Items;
    
    /// <summary>
    /// Calculated total of all line items
    /// </summary>
    public decimal Total => _invoice.Total;

    
    /// <summary>
    /// Prints the invoice and saves it to storage
    /// </summary>
    [RelayCommand]
    public async Task PrintAndSaveAsync()
    {
        var printSuccess = await HandlePrintingAsync();
        
        if (printSuccess)
        {
            await SaveInvoiceAsync();
        }
    }
    
    /// <summary>
    /// Handles the printing process for the invoice
    /// </summary>
    /// <returns>True if printing was successful or canceled, false if failed</returns>
    private async Task<bool> HandlePrintingAsync()
    {
        var printWindow = FindPrintPreviewWindow();
        if (printWindow == null)
            return false;

        var paperVisual = FindPaperVisual(printWindow);
        if (paperVisual == null)
        {
            ShowMessage("Print preview not found.", "Error", MessageBoxImage.Warning);
            return false;
        }

        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
        {
            ShowMessage("Print canceled.", "Notice", MessageBoxImage.Information);
            return true; // User canceled, not an error
        }

        return await ExecutePrintAsync(printDialog, paperVisual);
    }
    
    /// <summary>
    /// Finds the print preview window in the application
    /// </summary>
    /// <returns>Print preview window if found, null otherwise</returns>
    private static Window? FindPrintPreviewWindow()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window.Title.Equals("Invoice Print Preview", StringComparison.OrdinalIgnoreCase))
            {
                return window;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Finds the paper visual element for printing
    /// </summary>
    /// <param name="window">Window to search in</param>
    /// <returns>Paper visual element if found, null otherwise</returns>
    private static FrameworkElement? FindPaperVisual(Window window) =>
        LogicalTreeHelper.FindLogicalNode(window, "PaperVisual") as FrameworkElement;
    
    /// <summary>
    /// Executes the actual printing operation
    /// </summary>
    /// <param name="printDialog">Configured print dialog</param>
    /// <param name="paperVisual">Visual element to print</param>
    /// <returns>True if printing succeeded, false otherwise</returns>
    private async Task<bool> ExecutePrintAsync(PrintDialog printDialog, FrameworkElement paperVisual)
    {
        try
        {
            var originalTransform = paperVisual.LayoutTransform;
            ConfigurePrintLayout(printDialog, paperVisual);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                printDialog.PrintVisual(paperVisual, "Invoice Print");
            });

            // Restore original layout
            paperVisual.LayoutTransform = originalTransform;

            ShowMessage($"Printed to: {printDialog.PrintQueue.Name}", "Print Complete", MessageBoxImage.Information);
            return true;
        }
        catch (Exception ex)
        {
            ShowMessage($"Printing failed: {ex.Message}", "Error", MessageBoxImage.Error);
            return false;
        }
    }
    
    /// <summary>
    /// Configures the visual element layout for printing
    /// </summary>
    /// <param name="printDialog">Print dialog with printer capabilities</param>
    /// <param name="paperVisual">Visual element to configure</param>
    private static void ConfigurePrintLayout(PrintDialog printDialog, FrameworkElement paperVisual)
    {
        var capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
        var printableWidth = capabilities.PageImageableArea.ExtentWidth;
        var printableHeight = capabilities.PageImageableArea.ExtentHeight;

        // Measure the visual with unlimited space
        paperVisual.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        paperVisual.Arrange(new Rect(new Point(0, 0), paperVisual.DesiredSize));
        paperVisual.UpdateLayout();

        // Calculate scale to fit on printer page
        var scale = Math.Min(printableWidth / paperVisual.DesiredSize.Width, 
                           printableHeight / paperVisual.DesiredSize.Height);

        // Apply scaling transform
        paperVisual.LayoutTransform = new ScaleTransform(scale, scale);

        // Remeasure with scaled size
        var scaledSize = new Size(paperVisual.DesiredSize.Width * scale, 
                                paperVisual.DesiredSize.Height * scale);
        paperVisual.Measure(scaledSize);
        paperVisual.Arrange(new Rect(new Point(0, 0), scaledSize));
        paperVisual.UpdateLayout();
    }
    
    /// <summary>
    /// Saves the invoice to storage
    /// </summary>
    private async Task SaveInvoiceAsync()
    {
        try
        {
            await _storageService.SaveInvoiceAsync(_invoice);
            ShowMessage("Save complete!", "Save Success", MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowMessage($"Failed to save invoice: {ex.Message}", "Error", MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Shows a message box to the user
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="title">Window title</param>
    /// <param name="icon">Message box icon</param>
    private static void ShowMessage(string message, string title, MessageBoxImage icon) =>
        MessageBox.Show(message, title, MessageBoxButton.OK, icon);
}
