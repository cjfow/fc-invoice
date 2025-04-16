using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace FCInvoiceUI.Models;

/// <summary>
/// Represents a customer invoice with header information and a list of invoice items.
/// </summary>
public class BillingInvoice : INotifyPropertyChanged
{
    private DateTime _selectedDate = DateTime.Today;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (_selectedDate != value)
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
            }
        }
    }

    private uint _invoiceNumber;
    public uint InvoiceNumber
    {
        get => _invoiceNumber;
        set
        {
            if (_invoiceNumber != value)
            {
                _invoiceNumber = value;
                OnPropertyChanged(nameof(InvoiceNumber));
            }
        }
    }

    private string? _projectNumber;
    public string? ProjectNumber
    {
        get => _projectNumber;
        set
        {
            if (_projectNumber != value)
            {
                _projectNumber = value;
                OnPropertyChanged(nameof(ProjectNumber));
            }
        }
    }

    private string? _billTo;
    public string? BillTo
    {
        get => _billTo;
        set
        {
            if (_billTo != value)
            {
                _billTo = value;
                OnPropertyChanged(nameof(BillTo));
            }
        }
    }

    public ObservableCollection<InvoiceItem> Items { get; set; } = [];

    public decimal Total => Items.Sum(i => i.Amount);

    public BillingInvoice()
    {
        foreach (var item in Items)
        {
            item.PropertyChanged += OnItemChanged;
        }

        Items.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
            {
                foreach (InvoiceItem item in e.NewItems)
                    item.PropertyChanged += OnItemChanged;
            }

            if (e.OldItems != null)
            {
                foreach (InvoiceItem item in e.OldItems)
                    item.PropertyChanged -= OnItemChanged;
            }

            OnPropertyChanged(nameof(Total));
        };
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(InvoiceItem.Amount))
        {
            OnPropertyChanged(nameof(Total));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
