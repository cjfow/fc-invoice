using System.ComponentModel;

namespace FCInvoice.Core.Models;

/// <summary>
/// Represents a single line item on an invoice with quantity, description, and rate
/// </summary>
public class InvoiceItem : INotifyPropertyChanged
{
    private ushort? _quantity;
    
    /// <summary>
    /// Quantity of items for this line item
    /// </summary>
    public ushort? Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity == value)
                return;

            _quantity = value;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(Amount));
        }
    }

    private string? _description;
    
    /// <summary>
    /// Description of the work or item being billed
    /// </summary>
    public string? Description
    {
        get => _description;
        set
        {
            if (_description == value)
                return;

            _description = value;
            OnPropertyChanged(nameof(Description));
        }
    }

    private decimal? _rate;
    
    /// <summary>
    /// Rate per unit for this line item
    /// </summary>
    public decimal? Rate
    {
        get => _rate;
        set
        {
            if (_rate == value)
                return;

            _rate = value;
            OnPropertyChanged(nameof(Rate));
            OnPropertyChanged(nameof(Amount));
        }
    }

    /// <summary>
    /// Calculated total amount for this line item (Quantity * Rate)
    /// Defaults to 1 quantity if rate is provided but quantity is null
    /// </summary>
    public decimal Amount
    {
        get
        {
            // Default to quantity of 1 if rate exists but quantity is null, otherwise 0
            var effectiveQuantity = Quantity ?? (Rate.HasValue ? (ushort)1 : (ushort)0);
            return effectiveQuantity * (Rate ?? 0m);
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