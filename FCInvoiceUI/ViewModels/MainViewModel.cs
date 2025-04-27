using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCInvoiceUI.Models;
using FCInvoiceUI.Services;
using FCInvoiceUI.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FCInvoiceUI.ViewModels;

// TODO: abstract some functionality to services/helper classes. its getting a little congested..
public partial class MainViewModel : ObservableObject
{
    private readonly ComboBoxFormatService _comboBoxService;

    private readonly BillingInvoice _currentInvoiceHolder;
    private BillingInvoice? _originalInvoiceCache;

    public MainViewModel()
    {
        _comboBoxService = new ComboBoxFormatService();

        Invoice = new BillingInvoice
        {
            InvoiceNumber = InvoiceNumberGeneratorService.GetNextInvoiceNumber(),
            IsCurrentInvoice = true
        };

        _currentInvoiceHolder = Invoice;

        for (int i = 0; i < 3; i++)
        {
            Invoice.Items.Add(new InvoiceItem());
        }

        InitializeInvoiceEventHooks(Invoice);
        LoadComboBoxItems();
    }

    [ObservableProperty]
    private BillingInvoice _invoice;

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

    public ObservableCollection<InvoiceItem> InvoiceItems => Invoice.Items;

    public decimal Total => Invoice.Total;

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

    private void CacheCurrentInvoice()
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
            Invoice.InvoiceNumber = defaults.InvoiceNumber ?? InvoiceNumberGeneratorService.GetNextInvoiceNumber();
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

    [RelayCommand]
    private void OpenPrintPreview()
    {
        PrintView previewWindow = new(Invoice);
        previewWindow.ShowDialog();
    }
}