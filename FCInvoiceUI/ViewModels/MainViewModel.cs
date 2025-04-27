using CommunityToolkit.Mvvm.ComponentModel;
using FCInvoiceUI.Models;
using FCInvoiceUI.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FCInvoiceUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ComboBoxFormatService _comboBoxService;
    private BillingInvoice? _originalInvoiceCache;

    public MainViewModel()
    {
        _comboBoxService = new ComboBoxFormatService();

        Invoice = new BillingInvoice
        {
            InvoiceNumber = InvoiceNumberGeneratorService.GetNextInvoiceNumber()
        };

        InitializeInvoiceEventHooks(Invoice);

        LoadComboBoxItems();
    }

    [ObservableProperty]
    private BillingInvoice _invoice;

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

            if (value == Invoice)
            {
                RestoreOriginalInvoice();
                return;
            }

            CacheCurrentInvoice();
            OverwriteInvoice(value);
        }
    }

    private void LoadComboBoxItems()
    {
        var previousInvoices = _comboBoxService.LoadPreviousInvoices();

        FilteredInvoices.Clear();
        FilteredInvoices.Add(Invoice);
        foreach (var invoice in previousInvoices)
        {
            FilteredInvoices.Add(invoice);
        }

        SelectedInvoice = Invoice;
    }

    private void RestoreOriginalInvoice()
    {
        if (_originalInvoiceCache is null)
            return;

        CopyInvoice(_originalInvoiceCache);
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

    private void OverwriteInvoice(BillingInvoice source)
    {
        CopyInvoice(source);
    }

    private void CopyInvoice(BillingInvoice source)
    {
        Invoice.BillTo = source.BillTo;
        Invoice.ProjectNumber = source.ProjectNumber;
        Invoice.SelectedDate = source.SelectedDate;
        Invoice.InvoiceNumber = source.InvoiceNumber;

        Invoice.Items.Clear();
        foreach (var item in source.Items)
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
}