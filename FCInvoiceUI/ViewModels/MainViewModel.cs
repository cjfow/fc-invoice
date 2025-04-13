using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCInvoiceUI.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FCInvoiceUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        SelectedDate = DateTime.Today;

        InvoiceItems.Add(new InvoiceItem());
    }

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    private uint invoiceNumber;

    [ObservableProperty]
    private string? quantityText;

    [ObservableProperty]
    private string? rateText;

    [ObservableProperty]
    private string? projectNumber;

    [ObservableProperty]
    private string? billTo = "J.C. Concrete Inc.";

    public ObservableCollection<InvoiceItem> InvoiceItems { get; } = [];

    public decimal Total => InvoiceItems.Sum(item => item.Amount);

    [RelayCommand]
    private void PreviewPrint()
    {
        // TODO: make logic to preview or print the invoice.
       
    }

    [RelayCommand]
    private void LoadPreviousInvoices()
    {
        // TODO: Implement logic to load saved invoices from storage.
    }

    // this method is called when quantitytext changes
    partial void OnQuantityTextChanged(string? value)
    {
        // null check
        if (string.IsNullOrWhiteSpace(value))
            return;

        // try to parse the text as a number (we expect a whole number)
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal parsed))
            return;

        // round it to no decimal places (basically an integer)
        parsed = Math.Round(parsed, 0, MidpointRounding.AwayFromZero);
        // format the number with commas (thousands separators) and no decimals
        string formatted = parsed.ToString("N0", CultureInfo.CurrentCulture);

        if (formatted == value)
            return;

        // update quantitytext with the formatted value so the UI shows the correct format
        SetProperty(ref quantityText, formatted);
    }

    // this method is called when ratetext changes
    partial void OnRateTextChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        // try to parse the text as a number allowing currency formats
        if (!decimal.TryParse(value, NumberStyles.Currency | NumberStyles.Number, CultureInfo.CurrentCulture, out decimal parsed))
            return;

        // round the number to 2 decimal places
        parsed = Math.Round(parsed, 2, MidpointRounding.AwayFromZero);
        // format it with commas and two decimal places (for cents)
        string formatted = parsed.ToString("N2", CultureInfo.CurrentCulture);
        // if the formatted text is the same as the input, exit early
        if (formatted == value)
            return;

        // update ratetext with the formatted value so the UI shows it correctly
        SetProperty(ref rateText, formatted);
    }

}
