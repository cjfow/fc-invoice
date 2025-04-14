using CommunityToolkit.Mvvm.ComponentModel;

namespace FCInvoiceUI.Models
{
    /// <summary>
    /// Represents a single line item in an invoice.
    /// </summary>
    public partial class InvoiceItem : ObservableObject
    {
        // we use int? for quantity for simplicity—this means quantity is nullable.
        private int? _quantity;
        public int? Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        private decimal? _rate;
        public decimal? Rate
        {
            get => _rate;
            set
            {
                if (SetProperty(ref _rate, value))
                {
                    OnPropertyChanged(nameof(Amount));
                }
            }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public decimal Amount
        {
            get
            {
                // if Quantity is null but Rate exists, assume quantity 1,
                // otherwise if both are null, use 0
                int effectiveQuantity = Quantity ?? (Rate.HasValue ? 1 : 0);
                return effectiveQuantity * (Rate ?? 0m);
            }
        }
    }
}
