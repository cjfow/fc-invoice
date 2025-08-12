using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FCInvoice.Core.Models;

/// <summary>
/// Represents a complete billing invoice with customer info, items, and totals
/// </summary>
public class BillingInvoice : INotifyPropertyChanged
{
    /// <summary>
    /// Indicates if this is the current invoice being edited
    /// </summary>
    public bool IsCurrentInvoice { get; set; }

    /// <summary>
    /// Display name for UI showing invoice number with current invoice indicator
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (IsCurrentInvoice)
                return InvoiceNumber is not null ? $"Current Invoice ({InvoiceNumber})" : "Current Invoice";
            
            return InvoiceNumber ?? "Unnamed Invoice";
        }
    }

    private DateTime _selectedDate = DateTime.Today;
    
    /// <summary>
    /// Date for this invoice
    /// </summary>
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

    private string? _invoiceNumber;
    
    /// <summary>
    /// Unique invoice number in YYYYNNN format
    /// </summary>
    public string? InvoiceNumber
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

    private string? _projectNumber = $"{DateTime.Today:yy}-000";
    
    /// <summary>
    /// Project number associated with this invoice
    /// </summary>
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

    private string? _billTo = "J.C. Concrete Inc.";
    
    /// <summary>
    /// Customer/company name to bill for this invoice
    /// </summary>
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

    /// <summary>
    /// Collection of line items on this invoice
    /// </summary>
    public ObservableCollection<InvoiceItem> Items { get; set; } = [];

    /// <summary>
    /// Calculated total of all line items on this invoice
    /// </summary>
    public decimal Total => Items.Sum(i => i.Amount);

    public BillingInvoice()
    {
        // Set up existing item change tracking
        foreach (var item in Items)
        {
            item.PropertyChanged += OnItemChanged;
        }

        // Track collection changes to update total and wire up property change events
        Items.CollectionChanged += (_, e) =>
        {
            // Wire up property changed events for new items
            if (e.NewItems != null)
            {
                foreach (InvoiceItem item in e.NewItems)
                {
                    item.PropertyChanged += OnItemChanged;
                }
            }

            // Remove property changed events for removed items
            if (e.OldItems != null)
            {
                foreach (InvoiceItem item in e.OldItems)
                {
                    item.PropertyChanged -= OnItemChanged;
                }
            }

            OnPropertyChanged(nameof(Total));
        };
    }

    /// <summary>
    /// Handles property changes on invoice items to update total when amounts change
    /// </summary>
    /// <param name="sender">The invoice item that changed</param>
    /// <param name="e">Property change details</param>
    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(InvoiceItem.Amount))
        {
            OnPropertyChanged(nameof(Total));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifies the UI that a property value has changed for data binding updates
    /// </summary>
    /// <param name="propertyName">Name of the property that changed</param>
    private void OnPropertyChanged(string propertyName) => 
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}