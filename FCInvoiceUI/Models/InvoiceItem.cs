using System;
using System.ComponentModel;

namespace FCInvoiceUI.Models;

public class InvoiceItem : INotifyPropertyChanged
{
    private ushort? _quantity;

    public ushort? Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity == value) return;
            _quantity = value;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(Amount));
        }
    }

    private string? _description;

    public string? Description
    {
        get => _description;
        set
        {
            if (_description == value) return;
            _description = value;
            OnPropertyChanged(nameof(Description));
        }
    }

    private decimal? _rate;

    public decimal? Rate
    {
        get => _rate;
        set
        {
            if (_rate == value) return;
            _rate = value;
            OnPropertyChanged(nameof(Rate));
            OnPropertyChanged(nameof(Amount));
        }
    }


    public decimal Amount
    {
        get
        {
            var q = Quantity ?? (Rate.HasValue ? (ushort)1 : (ushort)0);
            return q * (Rate ?? 0m);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}
