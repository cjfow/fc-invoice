using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCInvoice.Core.Interfaces;
using FCInvoice.Core.Models;
using FCInvoice.Core.Services;
using FCInvoice.UI.Services;
using FCInvoice.UI.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FCInvoice.UI.ViewModels;

/// <summary>
/// Main view model for invoice creation and editing functionality
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ComboBoxFormatService _comboBoxService;
    private readonly IInvoiceNumberGenerator _invoiceNumberGenerator;
    private readonly BillingInvoice _currentInvoiceHolder;
    private BillingInvoice? _originalInvoiceCache;

    public MainViewModel() : this(new ComboBoxFormatService(), new InvoiceNumberGeneratorService()) { }
    
    public MainViewModel(ComboBoxFormatService comboBoxService) : this(comboBoxService, new InvoiceNumberGeneratorService()) { }
    
    public MainViewModel(ComboBoxFormatService comboBoxService, IInvoiceNumberGenerator invoiceNumberGenerator)
    {
        _comboBoxService = comboBoxService;
        _invoiceNumberGenerator = invoiceNumberGenerator;

        Invoice = CreateNewInvoice();
        _currentInvoiceHolder = Invoice;

        InitializeInvoiceEventHooks(Invoice);
        LoadComboBoxItems();
    }
    
    /// <summary>
    /// Creates a new invoice with default values and empty line items
    /// </summary>
    /// <returns>New billing invoice ready for editing</returns>
    private BillingInvoice CreateNewInvoice()
    {
        var invoice = new BillingInvoice
        {
            InvoiceNumber = _invoiceNumberGenerator.GetNextInvoiceNumber(),
            IsCurrentInvoice = true
        };

        // Add three empty line items by default
        for (int i = 0; i < 3; i++)
        {
            invoice.Items.Add(new InvoiceItem());
        }

        return invoice;
    }

    [ObservableProperty]
    private BillingInvoice _invoice;

    /// <summary>
    /// Customer/company name to bill for this invoice
    /// </summary>
    public string? BillTo
    {
        get => Invoice.BillTo;
        set
        {
            if (Invoice.BillTo != value)
            {
                Invoice.BillTo = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Project number associated with this invoice
    /// </summary>
    public string? ProjectNumber
    {
        get => Invoice.ProjectNumber;
        set
        {
            if (Invoice.ProjectNumber != value)
            {
                Invoice.ProjectNumber = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Date for this invoice
    /// </summary>
    public DateTime SelectedDate
    {
        get => Invoice.SelectedDate;
        set
        {
            if (Invoice.SelectedDate != value)
            {
                Invoice.SelectedDate = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Unique invoice number in YYYYNNN format
    /// </summary>
    public string? InvoiceNumber
    {
        get => Invoice.InvoiceNumber;
        set
        {
            if (Invoice.InvoiceNumber != value)
            {
                Invoice.InvoiceNumber = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Collection of line items on the current invoice
    /// </summary>
    public ObservableCollection<InvoiceItem> InvoiceItems => Invoice.Items;
    
    /// <summary>
    /// Calculated total of all line items on the current invoice
    /// </summary>
    public decimal Total => Invoice.Total;
    
    /// <summary>
    /// Collection of invoices available in the dropdown selector
    /// </summary>
    public ObservableCollection<BillingInvoice> FilteredInvoices { get; } = [];

    private BillingInvoice? _selectedInvoice;
    public BillingInvoice? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (!SetProperty(ref _selectedInvoice, value) || value is null)
                return;

            if (ReferenceEquals(value, _currentInvoiceHolder))
            {
                RestoreOriginalInvoice();
            }
            else
            {
                CacheCurrentInvoice();
                CopyInvoice(value, overwriteInvoiceNumber: true);
            }
        }
    }

    private void LoadComboBoxItems()
    {
        var previousInvoices = _comboBoxService.LoadPreviousInvoices();

        FilteredInvoices.Clear();
        FilteredInvoices.Add(_currentInvoiceHolder);

        foreach (var invoice in previousInvoices.OrderByDescending(i => i.InvoiceNumber))
        {
            FilteredInvoices.Add(invoice);
        }

        SelectedInvoice = _currentInvoiceHolder;
    }

    private void RestoreOriginalInvoice()
    {
        if (_originalInvoiceCache is not null)
        {
            CopyInvoice(_originalInvoiceCache);
        }
    }

    public void CacheCurrentInvoice()
    {
        _originalInvoiceCache = new BillingInvoice
        {
            BillTo = Invoice.BillTo,
            ProjectNumber = Invoice.ProjectNumber,
            SelectedDate = Invoice.SelectedDate,
            InvoiceNumber = Invoice.InvoiceNumber,
            Items = [..Invoice.Items.Select(i => new InvoiceItem
                {
                    Quantity = i.Quantity,
                    Description = i.Description,
                    Rate = i.Rate
                })]
        };
    }

    private void CopyInvoice(BillingInvoice defaults, bool overwriteInvoiceNumber = false)
    {
        Invoice.BillTo = defaults.BillTo ?? "J.C. Concrete Inc.";
        Invoice.ProjectNumber = defaults.ProjectNumber ?? $"{DateTime.Today:yy}-000";
        Invoice.SelectedDate = defaults.SelectedDate;

        if (overwriteInvoiceNumber && !Invoice.IsCurrentInvoice)
        {
            Invoice.InvoiceNumber = defaults.InvoiceNumber ?? _invoiceNumberGenerator.GetNextInvoiceNumber();
        }

        Invoice.Items.Clear();
        foreach (var item in defaults.Items)
        {
            var newItem = new InvoiceItem
            {
                Quantity = item.Quantity,
                Description = item.Description,
                Rate = item.Rate
            };
            newItem.PropertyChanged += InvoiceItem_PropertyChanged;
            Invoice.Items.Add(newItem);
        }

        OnPropertyChanged(nameof(InvoiceItems));
        OnPropertyChanged(nameof(Total));
    }

    private void InitializeInvoiceEventHooks(BillingInvoice invoice)
    {
        foreach (var item in invoice.Items)
        {
            item.PropertyChanged += InvoiceItem_PropertyChanged;
        }

        invoice.Items.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(InvoiceItems));

            if (e.NewItems is not null)
            {
                foreach (InvoiceItem item in e.NewItems)
                {
                    item.PropertyChanged += InvoiceItem_PropertyChanged;
                }
            }

            if (e.OldItems is not null)
            {
                foreach (InvoiceItem item in e.OldItems)
                {
                    item.PropertyChanged -= InvoiceItem_PropertyChanged;
                }
            }
        };
    }

    private void InvoiceItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(InvoiceItem.Amount))
        {
            OnPropertyChanged(nameof(Total));
        }
    }

    /// <summary>
    /// Opens the print preview window for the current invoice
    /// </summary>
    [RelayCommand]
    private void OpenPrintPreview()
    {
        try
        {
            PrintView previewWindow = new(Invoice);
            previewWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open print preview: {ex.Message}");
        }
    }
}