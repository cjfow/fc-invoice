using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FCInvoiceUI.Models;
using FCInvoiceUI.Services;
using FCInvoiceUI.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FCInvoiceUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        Invoice = new BillingInvoice
        {
            InvoiceNumber = InvoiceNumberGeneratorService.PeekNextInvoiceNumber()
        };

        for (int i = 0; i < 10; i++)
        {
            Invoice.Items.Add(new InvoiceItem());
        }

        foreach (var item in Invoice.Items)
        {
            item.PropertyChanged += InvoiceItem_PropertyChanged;
        }

        Invoice.Items.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Total));

            if (e.NewItems != null)
            {
                foreach (InvoiceItem item in e.NewItems)
                {
                    item.PropertyChanged += InvoiceItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (InvoiceItem item in e.OldItems)
                {
                    item.PropertyChanged -= InvoiceItem_PropertyChanged;
                }
            }
        };
    }

    [ObservableProperty]
    private BillingInvoice invoice;

    public ObservableCollection<InvoiceItem> InvoiceItems => Invoice.Items;

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

    public decimal Total => Invoice.Total;

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
        var previewWindow = new PrintView(Invoice)
        {
            DataContext = this
        };

        previewWindow.ShowDialog();
    }


    [RelayCommand]
    private void AddRow()
    {
        var newItem = new InvoiceItem();
        newItem.PropertyChanged += InvoiceItem_PropertyChanged;
        Invoice.Items.Add(newItem);
    }

    [RelayCommand]
    private void RemoveLastRow()
    {
        if (Invoice.Items.Count > 1)
        {
            var last = Invoice.Items[^1];
            last.PropertyChanged -= InvoiceItem_PropertyChanged;
            Invoice.Items.RemoveAt(Invoice.Items.Count - 1);
        }
    }
}
