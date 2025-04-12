
namespace FCInvoiceUI.Models;

/// <summary>
/// Represents a single line item in an invoice.
/// </summary>
public class InvoiceItem
{
    public ushort? Quantity { get; set; }

    public string? Description { get; set; }

    public decimal? Rate { get; set; }

    public decimal Amount
    {
        get
        {
            // if Rate exists and Quantity is null assume Quantity = 1.
            // side note: ?? is the null-coalescing operator, it basically means set the effectiveQuantity to Quantity if it exists,
            // if it doesnt exist, check if the rate has a value, if it does set the quantity to 1, if not set it to 0.
            ushort effectiveQuantity = Quantity ?? (Rate.HasValue ? (ushort)1 : (ushort)0);
            return effectiveQuantity * (Rate ?? 0m);
        }
    }
}
